using GreenfieldCoreDataAccess.Database.Models;

namespace GreenfieldCoreDataAccess.Database.Repositories.Interfaces;

public interface IUserRepository
{
    
    /// <summary>
    /// Get a user by their internal user ID
    /// </summary>
    /// <param name="userId">The internal user ID of the user to retrieve</param>
    /// <returns>The UserEntity if found, or null if no user was found with the given ID.</returns>
    Task<UserEntity?> GetUserByUserId(long userId);
    
    /// <summary>
    /// Get a user by their Minecraft UUID
    /// </summary>
    /// <param name="minecraftUuid">The Minecraft UUID of the user to retrieve</param>
    /// <returns></returns>
    Task<UserEntity?> GetUserByUuid(Guid minecraftUuid);
    
    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="minecraftUuid">The Minecraft UUID of the user to create</param>
    /// <param name="minecraftUsername">The Minecraft username of the user to create</param>
    /// <returns>The created UserEntity, or null if the user could not be created.</returns>
    Task<UserEntity?> CreateUser(Guid minecraftUuid, string minecraftUsername);
    
    /// <summary>
    /// Update a user's Minecraft username
    /// </summary>
    /// <param name="minecraftUuid">The Minecraft UUID of the user to update</param>
    /// <param name="newMinecraftUsername">The new Minecraft username to set</param>
    /// <returns>The previously stored UserEntity, or null if no user was found with the given UUID.</returns>
    Task<UserEntity?> UpdateUsername(Guid minecraftUuid, string newMinecraftUsername);
    
    /// <summary>
    /// Gets the system user
    /// </summary>
    /// <returns>The system UserEntity, will throw an exception if the user was not found.</returns>
    Task<UserEntity> GetSystemUser();
    
}