using System.Data;
using System.Transactions;

namespace GreenfieldCoreDataAccess.Database.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
 
    /// <summary>
    /// The database connection
    /// </summary>
    IDbConnection Connection { get; }
    
    /// <summary>
    /// The database transaction
    /// </summary>
    IDbTransaction? Transaction { get; set; }
    
    /// <summary>
    /// Get the requested repository
    /// </summary>
    /// <typeparam name="T">The type of repository</typeparam>
    /// <returns>The repository.</returns>
    T Repository<T>() where T : notnull;

    /// <summary>
    /// Whether there is an active transaction
    /// </summary>
    bool HasActiveTransaction => Transaction is not null;
    
    /// <summary>
    /// Begin a new transaction
    /// </summary>
    void BeginTransaction();
    
    /// <summary>
    /// Commit the transaction
    /// </summary>
    void Commit();
    
    /// <summary>
    /// Rollback the transaction
    /// </summary>
    void Rollback();

    /// <summary>
    /// Mark the unit of work as complete (allowing commit on dispose).
    /// Services should call this after all operations succeed.
    /// </summary>
    void Complete();
    
    /// <summary>
    /// Mark the unit of work as complete and commit the transaction.
    /// </summary>
    void CompleteAndCommit();

}