using System.Net;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Users;
using GreenfieldCoreServices.Services.Interfaces;

namespace GreenfieldCoreServices.Services;

public class UserService(IUnitOfWork uow, ICacheService<long, User> userCache) : IUserService
{
    public async Task<Result<User>> CreateUser(Guid minecraftUuid, string username)
    {
        if (!IsValidUsername(username))
            return Result<User>.Failure("A valid username must be provided.");
        var repo = uow.Repository<IUserRepository>();
        
        var didFindUser = userCache.TryGetValue(u => u.MinecraftUuid == minecraftUuid, out _) || (await repo.SelectUserByUuid(minecraftUuid)).GetOrThrow() is not null;
        if (didFindUser) return Result<User>.Failure("User already exists.", HttpStatusCode.Conflict);
        
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
            uow.BeginTransaction();

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

                var createResult = await repo.CreateUser(entry.Uuid, entry.Username);
                if (!createResult.IsSuccessful)
                {
                    skipped.Add(new BulkImportUserSkipped { Uuid = entry.Uuid, Reason = createResult.ErrorMessage ?? "Unknown error." });
                    continue;
                }

                var createdEntity = createResult.Data;
                if (createdEntity is null)
                {
                    skipped.Add(new BulkImportUserSkipped { Uuid = entry.Uuid, Reason = "User already exists." });
                    continue;
                }

                var createdUser = User.FromModel(createdEntity);
                created.Add(entry.Uuid);
                userCache.SetValue(createdUser.UserId, createdUser);
            }

            uow.CompleteAndCommit();
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

    public async Task<Result<User>> UpdateUsername(Guid minecraftUuid, string newUsername)
    {
        // Guid.Empty is the System user, can skip validation.
        if (!IsValidUsername(newUsername) && minecraftUuid != Guid.Empty)
            return Result<User>.Failure("A valid new username must be provided.");
        
        var repo = uow.Repository<IUserRepository>();
        
        var existingUserResult = await GetUserByUuid(minecraftUuid);
        if (!existingUserResult.IsSuccessful) return existingUserResult;
        var existingUser = existingUserResult.GetNonNullOrThrow();
        
        uow.BeginTransaction();
        var updateResult = await repo.UpdateUsername(minecraftUuid, newUsername);
        if (!updateResult.IsSuccessful) return Result<User>.Failure(updateResult.ErrorMessage ?? "Failed to update username.");
        uow.CompleteAndCommit();
        
        existingUser.Username = newUsername;
        userCache.SetValue(existingUser.UserId, existingUser);
        return Result<User>.Success(existingUser);
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