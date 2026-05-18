using GreenfieldCoreServices.Models.Users;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreServices.Services.Interfaces;

public interface IUserService
{
    
    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="minecraftUuid">The Minecraft UUID of the user to create</param>
    /// <param name="username">The Minecraft username of the user to create</param>
    /// <returns>The created User if an insert occurred; a failed Result otherwise.</returns>
    public Task<Result<User>> CreateUser(Guid minecraftUuid, string username);
    
    /// <summary>
    /// Bulk imports users in a single service call.
    /// </summary>
    /// <param name="users">The users to attempt to create.</param>
    /// <returns>A result containing created and skipped UUIDs.</returns>
    public Task<Result<BulkImportUsersResult>> BulkImportUsers(IEnumerable<BulkImportUserEntry> users);

    /// <summary>
    /// Get a user by their Minecraft UUID
    /// </summary>
    /// <param name="minecraftUuid">The Minecraft UUID of the user to retrieve</param>
    /// <returns>The User if found; a failed Result if no user was found with the given UUID.</returns>
    public Task<Result<User>> GetUserByUuid(Guid minecraftUuid);
    
    /// <summary>
    /// Get a user by their internal user ID
    /// </summary>
    /// <param name="userId">The internal user ID of the user to retrieve</param>
    /// <returns>The User if found; a failed Result if no user was found with the given ID.</returns>
    public Task<Result<User>> GetUserByUserId(long userId);
    
    /// <summary>
    /// Updates a user's Minecraft username.
    /// </summary>
    /// <param name="minecraftUuid">The Minecraft UUID of the user to update</param>
    /// <param name="newUsername">The new Minecraft username to set</param>
    /// <returns>All users updated as part of the operation (target and collision updates).</returns>
    public Task<Result<IEnumerable<User>>> UpdateUsername(Guid minecraftUuid, string newUsername);

    /// <summary>
    /// Refreshes usernames for the provided user IDs using Mojang profile data.
    /// Returns only users whose usernames were updated in the database.
    /// </summary>
    /// <param name="userIds">The user IDs to refresh.</param>
    /// <returns>All users updated as part of the refresh operation.</returns>
    public Task<Result<IEnumerable<User>>> RefreshUsernames(IEnumerable<long> userIds);

    /// <summary>
    /// Gets the system user, which is used for actions performed by the system rather than a specific user. This user has a Minecraft UUID of Guid.Empty and a username of "##System##".
    /// </summary>
    /// <returns></returns>
    public Task<Result<User>> GetSystemUser();

}