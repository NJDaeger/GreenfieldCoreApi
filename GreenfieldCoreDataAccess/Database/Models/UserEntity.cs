namespace GreenfieldCoreDataAccess.Database.Models;

public record UserEntity(long UserId, Guid MinecraftUuid, string MinecraftUsername, DateTime CreatedOn);