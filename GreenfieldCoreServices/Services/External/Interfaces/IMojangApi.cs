using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreServices.Services.External.Interfaces;

/// <summary>
/// Mojang API wrapper for retrieving the latest username by Minecraft UUID.
/// </summary>
public interface IMojangApi
{
    /// <summary>
    /// Resolves a player's current Minecraft username from Mojang profile services.
    /// </summary>
    /// <param name="minecraftUuid">The player's UUID.</param>
    Task<Result<string>> GetCurrentUsername(Guid minecraftUuid);
}

