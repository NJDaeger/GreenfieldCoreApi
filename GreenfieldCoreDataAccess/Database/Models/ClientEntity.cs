namespace GreenfieldCoreDataAccess.Database.Models;

public record ClientEntity(Guid ClientId, string ClientName, string Salt, DateTime CreatedOn);