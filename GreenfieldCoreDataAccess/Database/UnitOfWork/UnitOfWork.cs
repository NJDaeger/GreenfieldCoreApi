using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace GreenfieldCoreDataAccess.Database.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly Guid _instanceId = Guid.NewGuid();
    private readonly ILogger<IUnitOfWork> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITransactionScope _transactionScope;
    private bool _completed;

    public UnitOfWork(ILogger<IUnitOfWork> logger, ITransactionScope transactionScope, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _transactionScope = transactionScope;
        _logger.LogDebug("Creating unit of work: {InstanceId}", _instanceId);
    }

    /// <inheritdoc />
    public T Repository<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    /// <inheritdoc />
    public IDbConnection Connection => _transactionScope.Connection;

    /// <inheritdoc />
    public IDbTransaction? Transaction
    {
        get => _transactionScope.Transaction;
        set => _transactionScope.Transaction = value;
    }

    /// <inheritdoc />
    public void BeginTransaction()
    {
        if (Transaction is not null) throw new InvalidOperationException("Transaction already in progress.");
        _transactionScope.BeginTransaction();
        _logger.LogDebug("Beginning transaction for unit of work: {InstanceId}", _instanceId);
    }
    
    /// <inheritdoc />
    public void Commit()
    {
        if (Transaction is null) throw new InvalidOperationException("No transaction to commit.");
        if (!_completed) throw new InvalidOperationException("Transaction cannot be committed until unit of work is marked as complete.");
        _logger.LogDebug("Committing transaction for unit of work: {InstanceId}", _instanceId);
        try
        {
            Transaction!.Commit();
        }
        finally
        {
            Transaction!.Dispose();
            Transaction = null;
            _completed = false;
        }
    }

    /// <inheritdoc />
    public void Rollback()
    {
        if (Transaction is null) return;
        _logger.LogDebug("Rolling back transaction for unit of work: {InstanceId}", _instanceId);

        try
        {
            Transaction!.Rollback();
        }
        finally
        {
            Transaction!.Dispose();
            Transaction = null;
            _completed = false;
        }
    }

    /// <inheritdoc />
    public void Complete()
    {
        if (Transaction is null) throw new InvalidOperationException("No active transaction to complete.");
        _completed = true;
        _logger.LogDebug("Unit of work marked as complete: {InstanceId}", _instanceId);
    }

    /// <inheritdoc />
    public void CompleteAndCommit()
    {
        Complete();
        Commit();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        try
        {
            if (Transaction is not null)
            {
                if (_completed)
                {
                    try
                    {
                        _logger.LogDebug("Disposing unit of work with completed transaction, committing: {InstanceId}", _instanceId);
                        Commit();
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, "Commit failed during Dispose, attempting rollback.");
                        try
                        {
                            Rollback();
                        }
                        catch (Exception rollbackException)
                        {
                            _logger.LogError(rollbackException, "Rollback failed after commit failure.");
                        }
                        throw;
                    }
                }
                else
                {
                    _logger.LogDebug("Disposing unit of work with active but not completed transaction, rolling back: {InstanceId}", _instanceId);
                    try
                    {
                        Rollback();
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Rollback failed during Dispose.");
                        throw;
                    }
                }
            }
        }
        finally
        {
            _logger.LogDebug("Disposing unit of work: {InstanceId}", _instanceId);
            try
            {
                Connection.Close();
                Connection.Dispose();
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to close/dispose connection during UnitOfWork.Dispose.");
            }

            GC.SuppressFinalize(this);
        }
    }
}