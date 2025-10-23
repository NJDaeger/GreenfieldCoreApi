using GreenfieldCoreServices.Models;
using GreenfieldCoreServices.Models.Users;

namespace GreenfieldCoreServices.Services.Interfaces;

public interface IUserService
{
    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="minecraftUuid">The minecraft UUID of this user</param>
    /// <param name="username">The minecraft username of this user</param>
    /// <returns>Result containing the created user, or a bad result with a null user if the user could not be created.</returns>
    public Task<Result<User?>> CreateUser(Guid minecraftUuid, string username);
    
    /// <summary>
    /// Get a user by their minecraft uuid
    /// </summary>
    /// <param name="minecraftUuid">The user's minecraft uuid</param>
    /// <returns>The user if </returns>
    public Task<Result<User?>> GetUser(Guid minecraftUuid);

    /// <summary>
    /// Find a user by their username
    /// </summary>
    /// <param name="username">The username to find the user by</param>
    /// <returns>A list of users who have a matching username, or an empty list if no users have a matching username.</returns>
    public Task<Result<IEnumerable<User>>> FindUser(string username);

}