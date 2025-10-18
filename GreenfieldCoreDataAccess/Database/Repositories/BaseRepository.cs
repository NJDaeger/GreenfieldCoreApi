using System.Data;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreDataAccess.Database.Repositories;

public class BaseRepository(IUnitOfWork uow)
{
    internal IDbConnection Connection => uow.Connection;

    internal IDbTransaction? Transaction => uow.Transaction;
}