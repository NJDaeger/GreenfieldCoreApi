using System.Net;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Users;
using GreenfieldCoreServices.Services.External.Interfaces;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Services;

public class UserService(IUnitOfWork uow, ICacheService<long, User> userCache, IMojangApi mojangApi, ILogger<IUserService> logger) : IUserService
{
    public async Task<Result<User>> CreateUser(Guid minecraftUuid, string username)
    {
        if (!IsValidUsername(username))
            return Result<User>.Failure("A valid username must be provided.");
        var repo = uow.Repository<IUserRepository>();
        
        var didFindUser = userCache.TryGetValue(u => u.MinecraftUuid == minecraftUuid, out _) || (await repo.SelectUserByUuid(minecraftUuid)).GetOrThrow() is not null;
        if (didFindUser) return Result<User>.Failure("User already exists.", HttpStatusCode.Conflict);

        var collisionRefreshResult = await ResolveCollisions(minecraftUuid, username);
        if (!collisionRefreshResult.IsSuccessful)
            logger.LogWarning("Collision resolution failed while creating user with UUID '{MinecraftUuid}' and username '{Username}': {ErrorMessage}", minecraftUuid, username, collisionRefreshResult.ErrorMessage);
        
        uow.BeginTransaction();
        var created = (await repo.CreateUser(minecraftUuid, username)).GetOrThrow();
        if (created is null) return Result<User>.Failure("User could not be created.");
        uow.CompleteAndCommit();
        var createdUser = User.FromModel(created);
        userCache.SetValue(createdUser.UserId, createdUser);
        return Result<User>.Success(createdUser);
    }

    public async Task<Result<BulkImportUsersResult>> BulkImportUsers(IEnumerable<BulkImportUserEntry> users)
    {
        var created = new List<Guid>();
        var skipped = new List<BulkImportUserSkipped>();
        var seenUuids = new HashSet<Guid>();
        var repo = uow.Repository<IUserRepository>();

        try
        {

            foreach (var entry in users)
            {
                if (!seenUuids.Add(entry.Uuid))
                {
                    skipped.Add(new BulkImportUserSkipped { Uuid = entry.Uuid, Reason = "Duplicate UUID in request." });
                    continue;
                }

                if (!IsValidUsername(entry.Username))
                {
                    skipped.Add(new BulkImportUserSkipped { Uuid = entry.Uuid, Reason = "A valid username must be provided." });
                    continue;
                }

                var collisionRefreshResult = await ResolveCollisions(entry.Uuid, entry.Username);
                if (!collisionRefreshResult.IsSuccessful)
                {
                    skipped.Add(new BulkImportUserSkipped { Uuid = entry.Uuid, Reason = collisionRefreshResult.ErrorMessage ?? "Failed to refresh colliding usernames." });
                    continue;
                }

                uow.BeginTransaction();
                var createResult = await repo.CreateUser(entry.Uuid, entry.Username);
                uow.Complete();
                if (!createResult.IsSuccessful)
                {
                    uow.Rollback();
                    skipped.Add(new BulkImportUserSkipped { Uuid = entry.Uuid, Reason = createResult.ErrorMessage ?? "Unknown error." });
                    continue;
                }

                var createdEntity = createResult.Data;
                if (createdEntity is null)
                {
                    uow.Rollback();
                    skipped.Add(new BulkImportUserSkipped { Uuid = entry.Uuid, Reason = "User already exists." });
                    continue;
                }

                var createdUser = User.FromModel(createdEntity);
                created.Add(entry.Uuid);
                userCache.SetValue(createdUser.UserId, createdUser);
                uow.Commit();
            }
            return Result<BulkImportUsersResult>.Success(new BulkImportUsersResult
            {
                Created = created,
                Skipped = skipped
            });
        }
        catch (Exception ex)
        {
            if (uow.HasActiveTransaction)
                uow.Rollback();
            return Result<BulkImportUsersResult>.Failure($"Failed to bulk import users: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<User>> GetUserByUuid(Guid minecraftUuid)
    {
        if (userCache.TryGetValue(u => u.MinecraftUuid == minecraftUuid, out var cachedUser))
            return Result<User>.Success(cachedUser);
        
        var repo = uow.Repository<IUserRepository>();
        var foundUser = (await repo.SelectUserByUuid(minecraftUuid)).GetOrThrow();
        return foundUser is null ? Result<User>.Failure("User not found.", HttpStatusCode.NotFound) : Result<User>.Success(User.FromModel(foundUser));
    }

    public async Task<Result<User>> GetUserByUserId(long userId)
    {
        if (userCache.TryGetValue(userId, out var cachedUser))
            return Result<User>.Success(cachedUser);
        
        var repo = uow.Repository<IUserRepository>();
        var foundUser = (await repo.SelectUserByUserId(userId)).GetOrThrow();
        return foundUser is null ? Result<User>.Failure("User not found.", HttpStatusCode.NotFound) : Result<User>.Success(User.FromModel(foundUser));
    }

    public async Task<Result<IEnumerable<User>>> UpdateUsername(Guid minecraftUuid, string newUsername)
    {
        // Guid.Empty is the System user, can skip validation.
        if (!IsValidUsername(newUsername) && minecraftUuid != Guid.Empty)
            return Result<IEnumerable<User>>.Failure("A valid new username must be provided.");

        var repo = uow.Repository<IUserRepository>();

        var existingUserResult = await GetUserByUuid(minecraftUuid);
        if (!existingUserResult.IsSuccessful) 
            return Result<IEnumerable<User>>.Failure(existingUserResult.ErrorMessage ?? "Failed to retrieve user for update.");
        var existingUser = existingUserResult.GetNonNullOrThrow();

        var collisionRefreshResult = await ResolveCollisions(minecraftUuid, newUsername);
        if (!collisionRefreshResult.TryGetDataNonNull(out var updatedCollisions))
            logger.LogWarning("Collision resolution failed while updating username for user '{UserId}' with UUID '{MinecraftUuid}' to new username '{NewUsername}': {ErrorMessage}", existingUser.UserId, minecraftUuid, newUsername, collisionRefreshResult.ErrorMessage);

        uow.BeginTransaction();
        var updateResult = await repo.UpdateUsername(minecraftUuid, newUsername);
        if (!updateResult.IsSuccessful) return Result<IEnumerable<User>>.Failure(updateResult.ErrorMessage ?? "Failed to update username.");
        uow.CompleteAndCommit();

        existingUser.Username = newUsername;
        userCache.SetValue(existingUser.UserId, existingUser);
        return Result<IEnumerable<User>>.Success(new[] { existingUser }.Concat(updatedCollisions ?? []));
    }

    public async Task<Result<IEnumerable<User>>> RefreshUsernames(IEnumerable<long> userIds)
    {
        var updatedUsers = new List<User>();

        foreach (var userId in userIds)
        {
            var userResult = await GetUserByUserId(userId);
            if (!userResult.IsSuccessful)
            {
                logger.LogWarning("Failed to retrieve user with ID '{UserId}' for username refresh: {ErrorMessage}", userId, userResult.ErrorMessage);
                continue;
            }
            var user = userResult.GetNonNullOrThrow();
            if (user.MinecraftUuid == Guid.Empty)
            {
                logger.LogWarning("Skipping username refresh for system user with ID '{UserId}'.", userId);
                continue;
            }

            var mojangResult = await mojangApi.GetCurrentUsername(user.MinecraftUuid);
            if (!mojangResult.IsSuccessful)
            {
                logger.LogWarning("Failed to retrieve current username from Mojang for user '{UserId}' with UUID '{MinecraftUuid}': {ErrorMessage}", userId, user.MinecraftUuid, mojangResult.ErrorMessage);
                continue;
            }
            var currentUsername = mojangResult.GetNonNullOrThrow();
            if (currentUsername.Equals(user.Username, StringComparison.OrdinalIgnoreCase))
                continue;

            var updateResult = await UpdateUsername(user.MinecraftUuid, currentUsername);
            if (!updateResult.IsSuccessful)
            {
                logger.LogWarning("Failed to update username for user '{UserId}' with UUID '{MinecraftUuid}' during refresh: {ErrorMessage}", userId, user.MinecraftUuid, updateResult.ErrorMessage);
                continue;
            }
            updatedUsers.AddRange(updateResult.GetNonNullOrThrow());
        }
        return Result<IEnumerable<User>>.Success(updatedUsers);
    }

    private async Task<Result<List<User>>> GetCollidingUsers(string username)
    {
        var repo = uow.Repository<IUserRepository>();
        var result = await repo.SelectUsersByUsername(username);
        return !result.TryGetDataNonNull(out var collidingUsers)
            ? Result<List<User>>.Failure(result.ErrorMessage ?? "Failed to retrieve colliding users.", result.StatusCode)
            : Result<List<User>>.Success(collidingUsers.Select(User.FromModel).ToList());
    }

    private async Task<Result<IEnumerable<User>>> ResolveCollisions(Guid minecraftUuid, string username)
    {
        var collidingUsers = await GetCollidingUsers(username);
        if (!collidingUsers.TryGetDataNonNull(out var collidingUsersList))
            return Result<IEnumerable<User>>.Failure(collidingUsers.ErrorMessage ?? "Failed to retrieve colliding users for collision resolution.", collidingUsers.StatusCode);
        
        if (collidingUsersList.Count == 0 || (collidingUsersList.Count == 1 && collidingUsersList[0].MinecraftUuid == minecraftUuid))
            return Result<IEnumerable<User>>.Success([]);
        
        var allUpdatedUsers = new List<User>();
        
        foreach (var collidingUser in collidingUsersList)
        {
            logger.LogInformation("Found colliding user '{CollidingUserId}' with UUID '{CollidingUserUuid}' for username '{Username}'. Attempting to refresh username from Mojang.", collidingUser.UserId, collidingUser.MinecraftUuid, username);
            var updatedUsername = await mojangApi.GetCurrentUsername(collidingUser.MinecraftUuid);
            
            if (!updatedUsername.TryGetDataNonNull(out var refreshedUsername))
                return Result<IEnumerable<User>>.Failure(updatedUsername.ErrorMessage ?? $"Failed to refresh username from Mojang for user '{collidingUser.UserId}'.", updatedUsername.StatusCode);
            
            if (refreshedUsername.Equals(username, StringComparison.OrdinalIgnoreCase))
                return Result<IEnumerable<User>>.Failure($"Mojang returned the same username '{refreshedUsername}' for user '{collidingUser.UserId}', which is still colliding.", HttpStatusCode.Conflict);
            
            if (!IsValidUsername(refreshedUsername) && collidingUser.MinecraftUuid != Guid.Empty)
                return Result<IEnumerable<User>>.Failure($"Mojang returned an invalid username '{refreshedUsername}' for user '{collidingUser.UserId}'.", HttpStatusCode.BadGateway);
            
            var updatedUsers = await UpdateUsername(collidingUser.MinecraftUuid, refreshedUsername);
            if (!updatedUsers.TryGetDataNonNull(out var updatedUsersList))
                return Result<IEnumerable<User>>.Failure(updatedUsers.ErrorMessage ?? $"Failed to update username for colliding user '{collidingUser.UserId}' during collision resolution.", updatedUsers.StatusCode);
            
            allUpdatedUsers.AddRange(updatedUsersList);
        }
        return Result<IEnumerable<User>>.Success(allUpdatedUsers);
    }

    public async Task<Result<IEnumerable<User>>> GetAllUsers(bool skipCache = false)
    {
        if (!skipCache && userCache.GetCount() > 0)
            return Result<IEnumerable<User>>.Success(userCache.GetValues());

        var repo = uow.Repository<IUserRepository>();
        var result = await repo.SelectAllUsers();
        if (!result.TryGetDataNonNull(out var userEntities))
            return Result<IEnumerable<User>>.Failure(result.ErrorMessage ?? "Failed to retrieve users.");

        var users = userEntities.Select(User.FromModel).ToList();
        userCache.ClearCache();
        foreach (var user in users)
            userCache.SetValue(user.UserId, user);

        return Result<IEnumerable<User>>.Success(users);
    }

    public Task<Result<User>> GetSystemUser()
    {
        return GetUserByUuid(Guid.Empty);
    }
    
    private bool IsValidUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return false;
        return username.Length is >= 3 and <= 16 && username.All(c => char.IsLetterOrDigit(c) ||  c == '_');
    }
    
}