namespace GreenfieldCoreDataAccess.Database.Models;

public record ClientRoleEntity(long ClientRoleId, Guid ClientId, string RoleName, DateTime CreatedOn);