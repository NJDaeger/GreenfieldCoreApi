using System.Net;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Connections.Patreon;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Services;

public class PatreonService(IUnitOfWork uow, 
    ILogger<IPatreonService> logger,
    ICacheService<long, PatreonConnection> patreonConnectionCache, 
    ICacheService<(long userId, long patreonConnectionId), UserPatreonConnection> userPatreonConnectionCache) : IPatreonService
{
    public async Task<Result<IEnumerable<UserPatreonConnection>>> GetUserPatreonConnections(long userId)
    {
        if (userPatreonConnectionCache.TryGetValuesByPartialKey(key => key.userId == userId, out var cached))
            return Result<IEnumerable<UserPatreonConnection>>.Success(cached);
        
        var repo = uow.Repository<IPatreonConnectionRepository>();
        var selectResult = await repo.SelectUserPatreonConnections(userId);
        if (!selectResult.TryGetDataNonNull(out var userPatreonConnectionEntities))
            return Result<IEnumerable<UserPatreonConnection>>.Failure("Failed to retrieve user Patreon connections.", selectResult.StatusCode);

        var mapped = userPatreonConnectionEntities.Select(UserPatreonConnection.FromModel).ToList();
        foreach (var connection in mapped)
            userPatreonConnectionCache.SetValue((userId, connection.PatreonConnectionId), connection);
        
        return Result<IEnumerable<UserPatreonConnection>>.Success(mapped);
    }

    public async Task<Result<IEnumerable<PatreonConnection>>> GetAllPatreonConnections()
    {
        var repo = uow.Repository<IPatreonConnectionRepository>();
        var selectResult = await repo.SelectAllConnections();
        return selectResult.TryGetDataNonNull(out var accountEntities)
            ? Result<IEnumerable<PatreonConnection>>.Success(accountEntities.Select(PatreonConnection.FromModel))
            : Result<IEnumerable<PatreonConnection>>.Failure("Failed to retrieve Patreon connections.", selectResult.StatusCode);
    }

    public async Task<Result<PatreonConnection>> GetPatreonConnectionByPatreonId(long patreonId)
    {
        if (patreonConnectionCache.TryGetValue(c => c.PatreonId == patreonId, out var cached))
            return Result<PatreonConnection>.Success(cached);

        var repo = uow.Repository<IPatreonConnectionRepository>();
        var selectResult = await repo.SelectConnectionByPatreonId(patreonId);
        if (!selectResult.TryGetDataNonNull(out var accountEntity)) 
            return Result<PatreonConnection>.Failure("Patreon account not found.", HttpStatusCode.NotFound);
        
        var mappedAccount = PatreonConnection.FromModel(accountEntity);
        patreonConnectionCache.SetValue(mappedAccount.PatreonConnectionId, mappedAccount);
        
        return Result<PatreonConnection>.Success(mappedAccount);
    }

    public async Task<Result<PatreonConnection>> CreatePatreonConnection(string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry,
        string scope, long patreonId, string fullName, decimal? pledge)
    {
        var repo = uow.Repository<IPatreonConnectionRepository>();
        uow.BeginTransaction();
        
        var insertResult = await repo.InsertConnection(refreshToken, accessToken, tokenType, tokenExpiry, scope, patreonId, fullName, pledge);
        if (!insertResult.TryGetDataNonNull(out var entitiy))
            return Result<PatreonConnection>.Failure("Failed to create Patreon connection - " + insertResult.ErrorMessage, insertResult.StatusCode);
        uow.CompleteAndCommit();
        
        var mapped = PatreonConnection.FromModel(entitiy);
        patreonConnectionCache.SetValue(mapped.PatreonConnectionId, mapped);
        
        return Result<PatreonConnection>.Success(mapped);
    }

    public async Task<Result> DeletePatreonConnection(long patreonConnectionId)
    {
        var repo = uow.Repository<IPatreonConnectionRepository>();
        
        uow.BeginTransaction();
        var deleteResult = await repo.DeleteConnection(patreonConnectionId);
        if (!deleteResult.IsSuccessful)
            return Result.Failure("Failed to delete Patreon connection. " + deleteResult.ErrorMessage);
        uow.CompleteAndCommit();

        patreonConnectionCache.RemoveValue(patreonConnectionId);
        userPatreonConnectionCache.RemoveValues(udc => udc.PatreonConnectionId == patreonConnectionId);

        return Result.Success();
    }

    public async Task<Result<UserPatreonConnection>> LinkUserToPatreonConnection(long userId, long patreonConnectionId)
    {
        var repo = uow.Repository<IPatreonConnectionRepository>();
        
        uow.BeginTransaction();
        var insertResult = await repo.InsertUserPatreonConnection(userId, patreonConnectionId);
        if (!insertResult.TryGetDataNonNull(out var entity))
            return Result<UserPatreonConnection>.Failure("Failed to link user to Patreon connection.", insertResult.StatusCode);
        uow.CompleteAndCommit();

        var mapped = UserPatreonConnection.FromModel(entity);
        userPatreonConnectionCache.SetValue((userId, patreonConnectionId), mapped);

        return Result<UserPatreonConnection>.Success(mapped);
    }

    public async Task<Result> UnlinkUserPatreonConnection(long userId, long patreonConnectionId)
    {
        var repo = uow.Repository<IPatreonConnectionRepository>();

        uow.BeginTransaction();
        var deleteResult = await repo.DeleteUserPatreonConnection(userId, patreonConnectionId);
        if (!deleteResult.IsSuccessful)
            return Result.Failure("Failed to unlink user from Patreon connection. " + deleteResult.ErrorMessage);
        uow.CompleteAndCommit();

        userPatreonConnectionCache.RemoveValue((userId, patreonConnectionId));

        return Result.Success();
    }

    public async Task<Result<PatreonConnection>> UpdatePatreonConnectionTokens(long patreonConnectionId, string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope)
    {
        var repo = uow.Repository<IPatreonConnectionRepository>();
        
        uow.BeginTransaction();
        var updateResult = await repo.UpdateConnectionTokens(patreonConnectionId, refreshToken, accessToken, tokenType, tokenExpiry, scope);
        if (!updateResult.IsSuccessful)
            return Result<PatreonConnection>.Failure("Failed to update Patreon connection tokens. " + updateResult.ErrorMessage);
        uow.CompleteAndCommit();

        patreonConnectionCache.RemoveValue(patreonConnectionId);
        
        return await GetPatreonConnection(patreonConnectionId);
    }

    public async Task<Result<PatreonConnection>> UpdatePatreonConnectionProfile(long patreonConnectionId,
        string fullName, decimal? pledge)
    {
        var repo = uow.Repository<IPatreonConnectionRepository>();
        
        uow.BeginTransaction();
        var updateResult = await repo.UpdateConnectionProfile(patreonConnectionId, fullName, pledge);
        if (!updateResult.IsSuccessful)
            return Result<PatreonConnection>.Failure("Failed to update Patreon connection profile. " + updateResult.ErrorMessage);
        uow.CompleteAndCommit();

        patreonConnectionCache.RemoveValue(patreonConnectionId);
        
        return await GetPatreonConnection(patreonConnectionId);
    }

    public async Task<Result<UserPatreonConnection>> GetUserPatreonConnection(long userId, long patreonConnectionId)
    {
        if (userPatreonConnectionCache.TryGetValue((userId, patreonConnectionId), out var cached))
            return Result<UserPatreonConnection>.Success(cached);

        var repo = uow.Repository<IPatreonConnectionRepository>();
        var selectResult = await repo.SelectUserPatreonConnections(userId);
        if (!selectResult.TryGetDataNonNull(out var userPatreonConnectionEntities))
            return Result<UserPatreonConnection>.Failure("Failed to retrieve user Patreon connections.", selectResult.StatusCode);

        var mappedList = userPatreonConnectionEntities.Select(UserPatreonConnection.FromModel).ToList();
        foreach (var connection in mappedList)
            userPatreonConnectionCache.SetValue((userId, connection.PatreonConnectionId), connection);

        var mapped = mappedList.FirstOrDefault(c => c.PatreonConnectionId == patreonConnectionId);
        return mapped is null
            ? Result<UserPatreonConnection>.Failure("User Patreon connection not found.", HttpStatusCode.NotFound)
            : Result<UserPatreonConnection>.Success(mapped);
    }

    public async Task<Result<PatreonConnection>> GetPatreonConnection(long patreonConnectionId)
    {
        if (patreonConnectionCache.TryGetValue(patreonConnectionId, out var cached))
            return Result<PatreonConnection>.Success(cached);

        var repo = uow.Repository<IPatreonConnectionRepository>();
        var selectResult = await repo.SelectConnectionById(patreonConnectionId);
        if (!selectResult.TryGetDataNonNull(out var entity))
            return Result<PatreonConnection>.Failure("Patreon connection not found.", HttpStatusCode.NotFound);

        var mapped = PatreonConnection.FromModel(entity);
        patreonConnectionCache.SetValue(mapped.PatreonConnectionId, mapped);

        return Result<PatreonConnection>.Success(mapped);
    }

    public async Task<Result<IEnumerable<UserPatreonConnection>>> GetUsersByPatreonConnectionId(long patreonConnectionId)
    {
        if (userPatreonConnectionCache.TryGetValuesByPartialKey(key => key.patreonConnectionId == patreonConnectionId, out var cached))
            return Result<IEnumerable<UserPatreonConnection>>.Success(cached);

        var repo = uow.Repository<IPatreonConnectionRepository>();
        var selectResult = await repo.SelectUsersByPatreonConnection(patreonConnectionId);
        
        if (!selectResult.TryGetDataNonNull(out var userConnections))
            return Result<IEnumerable<UserPatreonConnection>>.Failure("Failed to retrieve user connections.", selectResult.StatusCode);

        return Result<IEnumerable<UserPatreonConnection>>.Success(userConnections.Select(ubpc => new UserPatreonConnection()
        {
            UserPatreonConnectionId = ubpc.UserPatreonConnectionId,
            UserId = ubpc.UserId,
            PatreonConnectionId = patreonConnectionId,
            ConnectedOn = ubpc.UserPatreonConnectionCreatedOn
        }));
    }
}