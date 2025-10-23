using GreenfieldCoreServices.Models.Clients;

namespace GreenfieldCoreServices.Services.Interfaces;

public interface IClientAuthService
{
    /// <summary>
    /// Registers a new client and returns a secret key.
    /// </summary>
    /// <param name="clientName">The name of the client to register</param>
    /// <param name="roles">Roles to assign to this user</param>
    /// <returns>A client and its secret</returns>
    Task<(Client client, string secret)> RegisterClient(string clientName, List<string> roles);

    /// <summary>
    /// Authenticates a client and returns a JWT token if successful.
    /// </summary>
    /// <param name="clientId">The ID of the client to authenticate</param>
    /// <param name="clientSecret">The secret of the client to authenticate</param>
    /// <returns>A JWT token. throws an exception if auth failed.</returns>
    Task<string> AuthenticateLogin(Guid clientId, string clientSecret);
    
    /// <summary>
    /// Gets all registered clients.
    /// </summary>
    /// <returns>An enumerable of registered clients.</returns>
    Task<IEnumerable<Client>> GetAllClients();
    
    /// <summary>
    /// Gets a client by their ID.
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    Task<Client?> GetClientById(Guid clientId);
    
    /// <summary>
    /// Gets a client by their name.
    /// </summary>
    /// <param name="clientName"></param>
    /// <returns></returns>
    Task<Client?> GetClientByName(string clientName);
    
    /// <summary>
    /// Deletes a client by their ID.
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    Task<Client?> DeleteClient(Guid clientId);
    
    /// <summary>
    /// Updates the roles assigned to a client.
    /// </summary>
    /// <param name="clientId">The ID of the client to update.</param>
    /// <param name="roles">The list of roles to assign to the client.</param>
    /// <returns></returns>
    Task<Client?> UpdateClientRoles(Guid clientId, List<string> roles);
    
    /// <summary>
    /// Refreshes a client's secret and returns the new secret.
    /// </summary>
    /// <param name="clientId">The ID of the client to refresh the secret for.</param>
    /// <returns>The new client secret, or null if the client was not found.</returns>
    Task<string?> RefreshClientSecret(Guid clientId);
    
    /// <summary>
    /// Updates a client's name.
    /// </summary>
    /// <param name="clientId">The ID of the client to update.</param>
    /// <param name="newName">The new name for the client.</param>
    /// <returns>The updated client, or null if the client was not found or could not be updated.</returns>
    Task<Client?> UpdateClientName(Guid clientId, string newName);

    /// <summary>
    /// Clears all roles assigned to a client.
    /// </summary>
    /// <param name="clientId">The ID of the client to clear roles for.</param>
    /// <returns></returns>
    Task<Client?> ClearClientRoles(Guid clientId);
}