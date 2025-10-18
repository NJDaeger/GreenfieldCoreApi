using System.Data;

namespace GreenfieldCoreDataAccess.Database.UnitOfWork;

public interface ITransactionScope : IDisposable
{
    IDbConnection Connection { get; }

    IDbTransaction? Transaction { get; set; }

    void BeginTransaction();
}