using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Clients;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GreenfieldCoreServices.Services;

public class ClientAuthService(IUnitOfWork uow, IConfiguration config) : IClientAuthService
{
    
    public async Task<(Client client, string secret)> RegisterClient(string clientName, List<string> roles)
    {
        var generatedSecret = GenerateClientSecret();
        
        uow.BeginTransaction();
        var repo = uow.Repository<IClientRepository>();
        
        var newClient = await repo.RegisterClient(clientName, generatedSecret.hashedSecret, generatedSecret.salt);

        var assignedRoles = new List<string>();
        foreach (var role in roles)
        {
            var assignedRole = await repo.AssignRoleToClient(newClient.Item1, role);
            if (assignedRole) assignedRoles.Add(role);
        }
        
        uow.CompleteAndCommit();
        
        return (new Client
        {
            ClientId = newClient.Item1,
            ClientName = clientName,
            CreatedOn = newClient.Item2,
            Roles = assignedRoles
        }, generatedSecret.secret);
    }

    public async Task<string> AuthenticateLogin(Guid clientId, string clientSecret)
    {
        var repo = uow.Repository<IClientRepository>();
        var client = await repo.GetClientById(clientId);
        var roles = (await repo.GetClientRoles(clientId)).Select(r => r.RoleName).ToList();
        
        if (client == null) throw new Exception("Client not found");
        
        var hashedSecret = HashClientSecret(clientSecret, client.Salt);
        
        var isValid = await repo.VerifyClientCredentials(clientId, hashedSecret.hash, hashedSecret.salt);
        
        return !isValid ? throw new Exception("Invalid credentials") : GenerateToken(clientId, roles);
    }

    public async Task<IEnumerable<Client>> GetAllClients()
    {
        var repo = uow.Repository<IClientRepository>();

        var foundClients = await repo.GetAllClients();
        
        var clients = new List<Client>();
        
        foreach (var client in foundClients)
        {
            var roles = (await repo.GetClientRoles(client.ClientId)).Select(r => r.RoleName).ToList();
            clients.Add(new Client
            {
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                CreatedOn = client.CreatedOn,
                Roles = roles
            });
        }
        return clients;
    }

    public async Task<Client?> GetClientById(Guid clientId)
    {
        var repo = uow.Repository<IClientRepository>();
        
        var foundClient = await repo.GetClientById(clientId);
        
        if (foundClient == null) return null;
        
        var roles = await repo.GetClientRoles(clientId);
        var roleNames = roles.Select(r => r.RoleName).ToList();
        
        return new Client
        {
            ClientId = foundClient.ClientId,
            ClientName = foundClient.ClientName,
            CreatedOn = foundClient.CreatedOn,
            Roles = roleNames
        };
    }

    public async Task<Client?> GetClientByName(string clientName)
    {
        var repo = uow.Repository<IClientRepository>();
        
        var foundClient = await repo.GetClientByName(clientName);
        
        if (foundClient == null) return null;
        
        var roles = await repo.GetClientRoles(foundClient.ClientId);
        var roleNames = roles.Select(r => r.RoleName).ToList();
        
        return new Client
        {
            ClientId = foundClient.ClientId,
            ClientName = foundClient.ClientName,
            CreatedOn = foundClient.CreatedOn,
            Roles = roleNames
        };
    }

    public async Task<Client?> DeleteClient(Guid clientId)
    {
        var foundClient = await GetClientById(clientId);
        if (foundClient == null) return null;
        
        uow.BeginTransaction();
        var deleteTask = await uow.Repository<IClientRepository>().DeleteClient(clientId);
        
        if (!deleteTask) return null;
        
        uow.CompleteAndCommit();
        
        return foundClient;
    }

    public async Task<Client?> UpdateClientRoles(Guid clientId, List<string> roles)
    {
        var foundClient = await GetClientById(clientId);
        if (foundClient == null) return null;
        
        var repo = uow.Repository<IClientRepository>();
        var currentRoles = foundClient.Roles;
        var rolesToAdd = roles.Except(currentRoles).ToList();
        var rolesToRemove = currentRoles.Except(roles).ToList();
        
        uow.BeginTransaction();
        
        foreach (var role in rolesToAdd)
        {
            var addedRole = await repo.AssignRoleToClient(clientId, role);
            if (addedRole) currentRoles.Add(role);
        }


        foreach (var role in rolesToRemove)
        {
            var removedRole = await repo.RemoveRoleFromClient(clientId, role);
            if (removedRole) currentRoles.Remove(role);
        }
        
        uow.CompleteAndCommit();
        
        foundClient.Roles = currentRoles;
        
        return foundClient;
    }

    public async Task<string?> RefreshClientSecret(Guid clientId)
    {
        var foundClient = await GetClientById(clientId);
        if (foundClient == null) return null;
        
        var newSecret = GenerateClientSecret();
        
        uow.BeginTransaction();
        
        await uow.Repository<IClientRepository>().UpdateClientSecret(clientId, newSecret.hashedSecret, newSecret.salt);
        
        uow.CompleteAndCommit();
        
        return newSecret.secret;
    }

    public async Task<Client?> UpdateClientName(Guid clientId, string newName)
    {
        var foundClient = await GetClientById(clientId);
        if (foundClient == null) return null;
        
        uow.BeginTransaction();
        
        var updateResult = await uow.Repository<IClientRepository>().UpdateClientName(clientId, newName);
        if (!updateResult) return null;
        
        uow.CompleteAndCommit();
        
        foundClient.ClientName = newName;
        return foundClient;
    }

    public async Task<Client?> ClearClientRoles(Guid clientId)
    {
        var foundClient = await GetClientById(clientId);
        if (foundClient == null) return null;
        
        uow.BeginTransaction();
        
        var removalCount = await uow.Repository<IClientRepository>().ClearClientRoles(clientId);
        
        if (removalCount == 0) return null;
        
        uow.CompleteAndCommit();

        if (foundClient.Roles.Count != removalCount) return await GetClientById(clientId);
        
        foundClient.Roles.Clear();
        return foundClient;
    }

    private static (string secret, string hashedSecret, string salt) GenerateClientSecret()
    {
        var clientSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var (hashedSecret, salt) = HashClientSecret(clientSecret);
        return (clientSecret, hashedSecret, salt);
    }

    private static (string hash, string salt) HashClientSecret(string clientSecret, string? salt = null)
    {
        var actualSalt = salt != null ? Convert.FromBase64String(salt) : RandomNumberGenerator.GetBytes(16);
        using var derived = new Rfc2898DeriveBytes(clientSecret, actualSalt, 10000, HashAlgorithmName.SHA256);
        var actualHash = derived.GetBytes(32);

        return (Convert.ToBase64String(actualHash), Convert.ToBase64String(actualSalt));
    }

    private string GenerateToken(Guid clientId, List<string> roles)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["jwtsettings:key"] ?? throw new ArgumentException("JWT key not found in configuration.")));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new System.Security.Claims.Claim(JwtRegisteredClaimNames.Sub, clientId.ToString()),
            new System.Security.Claims.Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        //add the roles of the client as claims
        var roleClaims = roles.Select(role => new System.Security.Claims.Claim("role", role));
        claims = claims.Concat(roleClaims).ToArray();
        
        var token = new JwtSecurityToken(config["jwtsettings:issuer"],
            config["jwtsettings:audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
}