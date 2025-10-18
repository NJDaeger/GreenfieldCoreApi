using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace GreenfieldCoreDataAccess.Database.UnitOfWork;

public class TransactionScope : ITransactionScope
{
    public IDbConnection Connection { get; }
    public IDbTransaction? Transaction { get; set; }
    private readonly ILogger<ITransactionScope> _logger;
    private bool _disposed;

    public TransactionScope(ILogger<ITransactionScope> logger, IConfiguration configuration)
    {
        _logger = logger;
        Connection = new MySqlConnection(configuration.GetConnectionString("GreenfieldCoreDb") ?? throw new ArgumentException("Connection string 'GreenfieldCoreDb' not found."));
        Connection.Open();
        _logger.LogDebug("TransactionScope opened a new connection.");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (Transaction is not null)
        {
            try
            {
                _logger.LogDebug("TransactionScope disposing with an active transaction; rolling back.");
                Transaction.Rollback();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed rolling back transaction in TransactionScope.Dispose.");
            }
            finally
            {
                try
                {
                    Transaction.Dispose();
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Failed disposing transaction in TransactionScope.Dispose.");
                }

                Transaction = null;
            }
        }

        try
        {
            Connection.Close();
            Connection.Dispose();
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed closing/disposing connection in TransactionScope.Dispose.");
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }

    public void BeginTransaction()
    {
        if (Transaction is not null) throw new InvalidOperationException("Transaction already in progress.");
        _logger.LogDebug("Beginning new transaction in TransactionScope.");
        Transaction = Connection.BeginTransaction();
    }
}