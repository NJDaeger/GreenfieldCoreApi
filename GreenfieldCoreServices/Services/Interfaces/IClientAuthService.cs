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
    
}