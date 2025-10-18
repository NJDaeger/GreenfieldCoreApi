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
        var clientSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hashedSecret = HashClientSecret(clientSecret);
        
        uow.BeginTransaction();
        
        var newClient = await uow.Repository<IClientRepository>().RegisterClient(clientName, hashedSecret.hash, hashedSecret.salt);

        var assignedRoles = new List<string>();
        var assignmentTasks = roles.Select(role =>
        {
            try
            {
                return uow.Repository<IClientRepository>().AssignRoleToClient(newClient.Item1, role);   
            }
            catch (Exception)
            {
                return Task.FromResult<ClientRoleEntity?>(null);
            }
        });
        var results = await Task.WhenAll(assignmentTasks);
        assignedRoles.AddRange(results.Where(r => r != null).Select(r => r!.RoleName));
        
        uow.CompleteAndCommit();
        
        return (new Client
        {
            ClientId = newClient.Item1,
            ClientName = clientName,
            CreatedOn = newClient.Item2,
            Roles = assignedRoles
        }, clientSecret);
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