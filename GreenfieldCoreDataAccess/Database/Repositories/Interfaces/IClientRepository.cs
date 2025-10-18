using GreenfieldCoreDataAccess.Database.Models;

namespace GreenfieldCoreDataAccess.Database.Repositories.Interfaces;

public interface IClientRepository
{
    /// <summary>
    /// Registers a new client and returns the generated ClientId (GUID).
    /// </summary>
    /// <param name="clientName"></param>
    /// <param name="clientSecretHash"></param>
    /// <param name="salt"></param>
    /// <returns></returns>
    Task<(Guid, DateTime)> RegisterClient(string clientName, string clientSecretHash, string salt);

    /// <summary>
    /// Verifies client credentials.
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="clientSecretHash"></param>
    /// <param name="salt"></param>
    /// <returns></returns>
    Task<bool> VerifyClientCredentials(Guid clientId, string clientSecretHash, string salt);
    
    /// <summary>
    /// Gets a client by their ID.
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    Task<ClientEntity?> GetClientById(Guid clientId);
    
    /// <summary>
    /// Gets all registered clients.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<ClientEntity>> GetAllClients();
    
    /// <summary>
    /// Deletes a client by their ID.
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    Task<bool> DeleteClient(Guid clientId);

    /// <summary>
    /// Gets all roles assigned to a client.
    /// </summary>
    /// <param name="clientId">Client who has roles</param>
    /// <returns>An enumerable of ClientRoles</returns>
    Task<IEnumerable<ClientRoleEntity>> GetClientRoles(Guid clientId);
    
    /// <summary>
    /// Assigns a role to a client.
    /// </summary>
    /// <param name="clientId">Client to add the role to</param>
    /// <param name="roleName">Name of the role to add</param>
    /// <returns>The assigned role. Null if no rule as assigned.</returns>
    Task<ClientRoleEntity?> AssignRoleToClient(Guid clientId, string roleName);
    
    /// <summary>
    /// Removes a role from a client.
    /// </summary>
    /// <param name="clientId">Client to remove the role from</param>
    /// <param name="roleName">Name of the role to remove</param>
    /// <returns>The removed role. Null if no role was removed.</returns>
    Task<ClientRoleEntity?> RemoveRoleFromClient(Guid clientId, string roleName);

}