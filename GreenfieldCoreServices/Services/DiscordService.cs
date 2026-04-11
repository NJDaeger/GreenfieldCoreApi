using System.Net;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Connections.Discord;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Services;

public class DiscordService(
    IUnitOfWork uow, 
    ILogger<IDiscordService> logger,
    ICacheService<long, DiscordConnection> discordConnectionCache, 
    ICacheService<(long userId, long discordConnectionId), UserDiscordConnection> userDiscordConnectionCache) : IDiscordService
{
    
    public async Task<Result<IEnumerable<DiscordConnection>>> GetAllDiscordConnections()
    {
        var repo = uow.Repository<IDiscordConnectionRepository>();
        var selectAllResult = await repo.SelectAllConnections();
        return selectAllResult.TryGetDataNonNull(out var accounts)
            ? Result<IEnumerable<DiscordConnection>>.Success(accounts.Select(DiscordConnection.FromModel))
            : Result<IEnumerable<DiscordConnection>>.Failure("Failed to retrieve Discord connections.", selectAllResult.StatusCode);
    }

    public async Task<Result<DiscordConnection>> GetDiscordConnectionBySnowflake(ulong discordSnowflake)
    {
        if (discordConnectionCache.TryGetValue(a => a.DiscordSnowflake == discordSnowflake, out var cached))
            return Result<DiscordConnection>.Success(cached);
        
        var repo = uow.Repository<IDiscordConnectionRepository>();
        var selectResult = await repo.SelectConnectionBySnowflake(discordSnowflake);
        if (!selectResult.TryGetDataNonNull(out var accountEnttiy))
            return Result<DiscordConnection>.Failure("Discord account not found.", HttpStatusCode.NotFound);
        
        var mappedAccount = DiscordConnection.FromModel(accountEnttiy);
        discordConnectionCache.SetValue(mappedAccount.DiscordConnectionId, mappedAccount);

        return Result<DiscordConnection>.Success(mappedAccount);
    }

    public async Task<Result<DiscordConnection>> GetDiscordConnection(long discordConnectionId)
    {
        if (discordConnectionCache.TryGetValue(discordConnectionId, out var cached))
            return Result<DiscordConnection>.Success(cached);

        var repo = uow.Repository<IDiscordConnectionRepository>();
        var selectResult = await repo.SelectConnectionById(discordConnectionId);
        if (!selectResult.TryGetDataNonNull(out var accountEntity))
            return Result<DiscordConnection>.Failure("Discord account not found.", HttpStatusCode.NotFound);

        var mappedAccount = DiscordConnection.FromModel(accountEntity);
        discordConnectionCache.SetValue(mappedAccount.DiscordConnectionId, mappedAccount);

        return Result<DiscordConnection>.Success(mappedAccount);
    }

    public async Task<Result<IEnumerable<UserDiscordConnection>>> GetUsersByDiscordConnectionId(long discordConnectionId)
    {
        if (userDiscordConnectionCache.TryGetValuesByPartialKey(key => key.discordConnectionId == discordConnectionId, out var cached))
            return Result<IEnumerable<UserDiscordConnection>>.Success(cached);
        
        var repo = uow.Repository<IDiscordConnectionRepository>();
        var selectResult = await repo.SelectUsersByDiscordConnection(discordConnectionId);
        
        if (!selectResult.TryGetDataNonNull(out var userConnections))
            return Result<IEnumerable<UserDiscordConnection>>.Failure("Failed to retrieve user connections.", selectResult.StatusCode);

        return Result<IEnumerable<UserDiscordConnection>>.Success(userConnections.Select(ubdc => new UserDiscordConnection()
        {
            UserDiscordConnectionId = ubdc.UserDiscordConnectionId,
            UserId = ubdc.UserId,
            DiscordConnectionId = discordConnectionId,
            ConnectedOn = ubdc.UserDiscordConnectionCreatedOn
        }));
    }

    public async Task<Result<DiscordConnection>> UpdateDiscordConnectionTokens(long discordConnectionId,
        string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope)
    {
        var repo = uow.Repository<IDiscordConnectionRepository>();
        uow.BeginTransaction();
        var updateResult = await repo.UpdateConnectionTokens(discordConnectionId, refreshToken, accessToken, tokenType, tokenExpiry, scope);
        if (!updateResult.IsSuccessful)
            return Result<DiscordConnection>.Failure("Failed to update Discord account tokens. " + updateResult.ErrorMessage);
        uow.CompleteAndCommit();

        discordConnectionCache.RemoveValue(discordConnectionId);
        return await GetDiscordConnection(discordConnectionId);
    }

    public async Task<Result<DiscordConnection>> UpdateDiscordConnectionProfile(long discordConnectionId, string discordUsername)
    {
        var repo = uow.Repository<IDiscordConnectionRepository>();
        uow.BeginTransaction();
        var updateResult = await repo.UpdateConnectionProfile(discordConnectionId, discordUsername);
        if (!updateResult.IsSuccessful)
            return Result<DiscordConnection>.Failure("Failed to update Discord account profile. " + updateResult.ErrorMessage);
        uow.CompleteAndCommit();

        discordConnectionCache.RemoveValues(a => a.DiscordConnectionId == discordConnectionId);
        return await GetDiscordConnection(discordConnectionId);;
    }

    public async Task<Result<DiscordConnection>> CreateDiscordConnection(string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope, ulong discordSnowflake, string discordUsername)
    {
        var repo = uow.Repository<IDiscordConnectionRepository>();
        uow.BeginTransaction();
        
        var insertResult = await repo.InsertConnection(refreshToken, accessToken, tokenType, tokenExpiry, scope, discordSnowflake, discordUsername);
        if (!insertResult.TryGetDataNonNull(out var entity))
            return Result<DiscordConnection>.Failure("Failed to create Discord connection - " + insertResult.ErrorMessage, insertResult.StatusCode);
        uow.CompleteAndCommit();

        var mapped = DiscordConnection.FromModel(entity);
        discordConnectionCache.SetValue(mapped.DiscordConnectionId, mapped);
        
        return Result<DiscordConnection>.Success(mapped);
    }

    public async Task<Result> DeleteDiscordConnection(long discordConnectionId)
    {
        var repo = uow.Repository<IDiscordConnectionRepository>();
        
        uow.BeginTransaction();
        var deleteResult = await repo.DeleteConnection(discordConnectionId);
        if (!deleteResult.IsSuccessful)
            return Result.Failure("Failed to delete Discord connection. " + deleteResult.ErrorMessage);
        uow.CompleteAndCommit();
        
        discordConnectionCache.RemoveValue(discordConnectionId);
        userDiscordConnectionCache.RemoveValues(udc => udc.DiscordConnectionId == discordConnectionId);
        
        return Result.Success();
    }

    public async Task<Result<UserDiscordConnection>> LinkUserToDiscordConnection(long userId, long discordConnectionId)
    {
        var repo = uow.Repository<IDiscordConnectionRepository>();
        
        uow.BeginTransaction();
        var insertResult = await repo.InsertUserDiscordConnection(userId, discordConnectionId);
        if (!insertResult.TryGetDataNonNull(out var entity))
            return Result<UserDiscordConnection>.Failure("Discord account could not be linked.");
        uow.CompleteAndCommit();

        var mapped = UserDiscordConnection.FromModel(entity);
        userDiscordConnectionCache.SetValue((userId, discordConnectionId), mapped);

        return Result<UserDiscordConnection>.Success(mapped);
    }

    public async Task<Result> UnlinkUserDiscordConnection(long userId, long discordConnectionId)
    {
        var repo = uow.Repository<IDiscordConnectionRepository>();
        
        uow.BeginTransaction();
        var deleteResult = await repo.DeleteUserDiscordConnection(userId, discordConnectionId);
        if (!deleteResult.IsSuccessful)
            return Result.Failure("Discord account could not be unlinked. " + deleteResult.ErrorMessage);
        uow.CompleteAndCommit();
        
        userDiscordConnectionCache.RemoveValue((userId, discordConnectionId));
        
        return Result.Success();
    }

    public async Task<Result<UserDiscordConnection>> GetUserDiscordConnection(long userId, long discordConnectionId)
    {
        if (userDiscordConnectionCache.TryGetValue((userId, discordConnectionId), out var cached)) 
            return Result<UserDiscordConnection>.Success(cached);
        
        var repo = uow.Repository<IDiscordConnectionRepository>();
        var selectResult = await repo.SelectUserDiscordConnections(userId);
        
        if (!selectResult.TryGetDataNonNull(out var entities))
            return Result<UserDiscordConnection>.Failure("Failed to retrieve Discord connection.");

        var mapped = entities.Select(UserDiscordConnection.FromModel).ToList();
        foreach (var account in mapped)
            userDiscordConnectionCache.SetValue((userId, account.DiscordConnectionId), account);
        
        var found = mapped.FirstOrDefault(a => a.DiscordConnectionId == discordConnectionId);
        return found is not null
            ? Result<UserDiscordConnection>.Success(found)
            : Result<UserDiscordConnection>.Failure("Discord connection not found.", HttpStatusCode.NotFound);
    }

    public async Task<Result<IEnumerable<UserDiscordConnection>>> GetUserDiscordConnections(long userId)
    {
        if (userDiscordConnectionCache.TryGetValuesByPartialKey(key => key.userId == userId, out var cached))
            return Result<IEnumerable<UserDiscordConnection>>.Success(cached);
        
        var repo = uow.Repository<IDiscordConnectionRepository>();
        var selectResult = await repo.SelectUserDiscordConnections(userId);
        if (!selectResult.TryGetDataNonNull(out var entities))
            return Result<IEnumerable<UserDiscordConnection>>.Failure("Failed to retrieve user Discord connections.");

        var mapped = entities.Select(UserDiscordConnection.FromModel).ToList();
        foreach (var account in mapped)
            userDiscordConnectionCache.SetValue((userId, account.DiscordConnectionId), account);

        return Result<IEnumerable<UserDiscordConnection>>.Success(mapped);
    }
}
