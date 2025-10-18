using System.Data;
using Dapper;
using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreDataAccess.Database.Repositories;

public class ClientRepository(IUnitOfWork uow) : BaseRepository(uow), IClientRepository
{
    
    private const string RegisterClientProc = "usp_RegisterClient";
    private const string VerifyClientCredentialsProc = "usp_VerifyClient";
    private const string GetClientByIdProc = "usp_GetClientById";
    private const string GetAllClientsProc = "usp_GetAllClients";
    private const string DeleteClientProc = "usp_DeleteClient";
    
    private const string GetClientRolesProc = "usp_SelectClientRoles";
    private const string AssignRoleToClientProc = "usp_InsertClientRole";
    private const string RemoveRoleFromClientProc = "usp_DeleteClientRole";
    

    /// <inheritdoc />
    public async Task<(Guid, DateTime)> RegisterClient(string clientName, string clientSecretHash, string salt)
    {
        var guid = Guid.NewGuid();
        
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", guid, System.Data.DbType.Guid);
        parameters.Add("p_ClientName", clientName, System.Data.DbType.String, size: 255);
        parameters.Add("p_ClientSecretHash", clientSecretHash, System.Data.DbType.String, size: 255);
        parameters.Add("p_Salt", salt, System.Data.DbType.String, size: 255);
        
        var createdOn = await Connection.ExecuteScalarAsync<DateTime>(RegisterClientProc, parameters, commandType: System.Data.CommandType.StoredProcedure, transaction: Transaction);
        
        return (guid, createdOn);
    }

    /// <inheritdoc />
    public async Task<bool> VerifyClientCredentials(Guid clientId, string clientSecretHash,
        string salt)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, System.Data.DbType.Guid);
        parameters.Add("p_ClientSecretHash", clientSecretHash, System.Data.DbType.String, size: 255);
        parameters.Add("p_Salt", salt, System.Data.DbType.String, size: 255);
        
        return await Connection.ExecuteScalarAsync<bool>(VerifyClientCredentialsProc, parameters, commandType: System.Data.CommandType.StoredProcedure, transaction: Transaction);
    }

    /// <inheritdoc />
    public async Task<ClientEntity?> GetClientById(Guid clientId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, System.Data.DbType.Guid);
        
        return await Connection.QuerySingleOrDefaultAsync<ClientEntity?>(GetClientByIdProc, parameters, commandType: System.Data.CommandType.StoredProcedure, transaction: Transaction);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ClientEntity>> GetAllClients()
    {
        return await Connection.QueryAsync<ClientEntity>(GetAllClientsProc, commandType: System.Data.CommandType.StoredProcedure, transaction: Transaction);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteClient(Guid clientId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, System.Data.DbType.Guid);

        return await Connection.ExecuteAsync(DeleteClientProc, parameters,
            commandType: System.Data.CommandType.StoredProcedure, transaction: Transaction) > 0;
    }

    public async Task<IEnumerable<ClientRoleEntity>> GetClientRoles(Guid clientId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, DbType.Guid);
        
        return await Connection.QueryAsync<ClientRoleEntity>(GetClientRolesProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
    }

    public Task<ClientRoleEntity?> AssignRoleToClient(Guid clientId, string roleName)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, DbType.Guid);
        parameters.Add("p_RoleName", roleName, DbType.String, size: 255);
        
        return Connection.QuerySingleOrDefaultAsync<ClientRoleEntity?>(AssignRoleToClientProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
    }

    public Task<ClientRoleEntity?> RemoveRoleFromClient(Guid clientId, string roleName)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ClientId", clientId, DbType.Guid);
        parameters.Add("p_RoleName", roleName, DbType.String, size: 255);
        
        return Connection.QuerySingleOrDefaultAsync<ClientRoleEntity?>(RemoveRoleFromClientProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
    }
}