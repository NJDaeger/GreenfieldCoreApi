using System.Data;
using Dapper;
using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreDataAccess.Database.Repositories;

public class ClientRepository(IUnitOfWork uow) : BaseRepository(uow), IClientRepository
{
    
    private const string DeleteClientProc = "usp_DeleteClient";
    private const string GetAllClientsProc = "usp_GetAllClients";
    private const string GetClientByIdProc = "usp_GetClientById";
    private const string GetClientByNameProc = "usp_GetClientByName";
    private const string RegisterClientProc = "usp_RegisterClient";
    private const string UpdateClientNameProc = "usp_UpdateClientName";
    private const string UpdateClientSecretProc = "usp_UpdateClientSecret";
    private const string VerifyClientCredentialsProc = "usp_VerifyClient";
    
    private const string ClearClientRolesProc = "usp_ClearClientRoles";
    private const string DeleteClientRoleProc = "usp_DeleteClientRole";
    private const string InsertClientRoleProc = "usp_InsertClientRole";
    private const string SelectClientRolesProc = "usp_SelectClientRoles";
    

    /// <inheritdoc />
    public async Task<(Guid, DateTime)> RegisterClient(string clientName, string clientSecretHash, string salt)
    {
        var guid = Guid.NewGuid();
        
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", guid, DbType.Guid);
        parameters.Add("p_ClientName", clientName, DbType.String, size: 255);
        parameters.Add("p_ClientSecretHash", clientSecretHash, DbType.String, size: 255);
        parameters.Add("p_Salt", salt, DbType.String, size: 255);
        
        var createdOn = await Connection.ExecuteScalarAsync<DateTime>(RegisterClientProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
        
        return (guid, createdOn);
    }

    /// <inheritdoc />
    public async Task<bool> VerifyClientCredentials(Guid clientId, string clientSecretHash,
        string salt)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, DbType.Guid);
        parameters.Add("p_ClientSecretHash", clientSecretHash, DbType.String, size: 255);
        parameters.Add("p_Salt", salt, DbType.String, size: 255);
        
        return await Connection.ExecuteScalarAsync<bool>(VerifyClientCredentialsProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
    }

    /// <inheritdoc />
    public async Task<ClientEntity?> GetClientById(Guid clientId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, DbType.Guid);
        
        return await Connection.QuerySingleOrDefaultAsync<ClientEntity?>(GetClientByIdProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
    }

    /// <inheritdoc />
    public Task<ClientEntity?> GetClientByName(string clientName)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientName", clientName, DbType.String, size: 255);
        
        return Connection.QuerySingleOrDefaultAsync<ClientEntity?>(GetClientByNameProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
    }

    /// <inheritdoc />
    public Task<IEnumerable<ClientEntity>> GetAllClients()
    {
        return Connection.QueryAsync<ClientEntity>(GetAllClientsProc, commandType: CommandType.StoredProcedure, transaction: Transaction);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteClient(Guid clientId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, DbType.Guid);

        return await Connection.ExecuteAsync(DeleteClientProc, parameters,
            commandType: CommandType.StoredProcedure, transaction: Transaction) > 0;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ClientRoleEntity>> GetClientRoles(Guid clientId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, DbType.Guid);
        
        return await Connection.QueryAsync<ClientRoleEntity>(SelectClientRolesProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
    }

    /// <inheritdoc />
    public async Task<bool> AssignRoleToClient(Guid clientId, string roleName)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, DbType.Guid);
        parameters.Add("p_RoleName", roleName, DbType.String, size: 255);

        return await Connection.ExecuteAsync(InsertClientRoleProc, parameters, commandType: CommandType.StoredProcedure,
            transaction: Transaction) > 0;
    }

    /// <inheritdoc />
    public async Task<bool> RemoveRoleFromClient(Guid clientId, string roleName)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, DbType.Guid);
        parameters.Add("p_RoleName", roleName, DbType.String, size: 255);

        return await Connection.ExecuteAsync(DeleteClientRoleProc, parameters, commandType: CommandType.StoredProcedure,
            transaction: Transaction) > 0;
    }

    /// <inheritdoc />
    public Task<int> ClearClientRoles(Guid clientId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, DbType.Guid);
        
        return Connection.ExecuteAsync(ClearClientRolesProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
    }

    /// <inheritdoc />
    public Task<bool> UpdateClientName(Guid clientId, string newClientName)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, DbType.Guid);
        parameters.Add("p_NewClientName", newClientName, DbType.String, size: 255);
        
        return Connection.ExecuteAsync(UpdateClientNameProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction)
            .ContinueWith(t => t.Result > 0);
    }

    /// <inheritdoc />
    public Task UpdateClientSecret(Guid clientId, string newClientSecretHash, string newSalt)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, DbType.Guid);
        parameters.Add("p_NewSecretHash", newClientSecretHash, DbType.String, size: 255);
        parameters.Add("p_NewSalt", newSalt, DbType.String, size: 255);
        
        return Connection.ExecuteAsync(UpdateClientSecretProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
    }
}