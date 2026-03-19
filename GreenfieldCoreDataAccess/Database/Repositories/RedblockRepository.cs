using System.Data.Common;
using System.Net;
using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.Procedures;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreDataAccess.Database.Repositories;

public class RedblockRepository(IUnitOfWork uow, ILogger<IRedblockRepository> logger) : BaseRepository(uow), IRedblockRepository
{
    public async Task<Result<RedblockProjectEntity>> InsertProject(string projectName, string projectKey)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.InsertProject, (projectName, projectKey), Transaction);
            return result is null
                ? Result<RedblockProjectEntity>.Failure("Failed to insert redblock project: No project returned from database.", HttpStatusCode.InternalServerError)
                : Result<RedblockProjectEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockProjectEntity>.Failure($"Failed to insert redblock project: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<RedblockProjectEntity>> UpdateProject(long projectId, string projectName)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.UpdateProject, (projectId, projectName), Transaction);
            return result is null
                ? Result<RedblockProjectEntity>.Failure("Redblock project not found.", HttpStatusCode.NotFound)
                : Result<RedblockProjectEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockProjectEntity>.Failure($"Failed to update redblock project: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<RedblockProjectEntity>>> SelectProjects()
    {
        try
        {
            var result = await Connection.QueryProcedure(StoredProcs.Redblocks.SelectProjects, Transaction);
            return Result<IEnumerable<RedblockProjectEntity>>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<IEnumerable<RedblockProjectEntity>>.Failure($"Failed to select redblock projects: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<RedblockProjectEntity>> SelectProjectById(long projectId)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.SelectProjectById, projectId, Transaction);
            return result is null
                ? Result<RedblockProjectEntity>.Failure("Redblock project not found.", HttpStatusCode.NotFound)
                : Result<RedblockProjectEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockProjectEntity>.Failure($"Failed to select redblock project: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<RedblockEntity>> InsertRedblock(long projectId, string message, int x, int y, int z, long createdBy)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.InsertRedblock, (projectId, message, x, y, z, createdBy), Transaction);
            return result is null
                ? Result<RedblockEntity>.Failure("Failed to insert redblock: No redblock returned from database.", HttpStatusCode.InternalServerError)
                : Result<RedblockEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockEntity>.Failure($"Failed to insert redblock: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<RedblockEntity>> SelectRedblockByKey(long projectId, long keyNumber)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.SelectRedblockByKey, (projectId, keyNumber), Transaction);
            return result is null
                ? Result<RedblockEntity>.Failure("Redblock not found.", HttpStatusCode.NotFound)
                : Result<RedblockEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockEntity>.Failure($"Failed to select redblock: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<RedblockEntity>>> SelectRedblocksByProject(long projectId, string? statusFilter, string? deletionFilter, string? userAssignmentFilter, string? roleAssignmentFilter, string? messageFilter)
    {
        try
        {
            var result = await Connection.QueryProcedure(StoredProcs.Redblocks.SelectRedblocksByProject,
                (projectId, statusFilter, deletionFilter, userAssignmentFilter, roleAssignmentFilter, messageFilter),
                Transaction);
            return Result<IEnumerable<RedblockEntity>>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<IEnumerable<RedblockEntity>>.Failure($"Failed to select redblocks by project: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<RedblockEntity>> UpdateRedblockMessage(long projectId, long keyNumber, string message)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.UpdateRedblockMessage, (projectId, keyNumber, message), Transaction);
            return result is null
                ? Result<RedblockEntity>.Failure("Redblock not found.", HttpStatusCode.NotFound)
                : Result<RedblockEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockEntity>.Failure($"Failed to update redblock message: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<RedblockEntity>> SoftDeleteRedblock(long projectId, long keyNumber, long deletedBy)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.SoftDeleteRedblock, (projectId, keyNumber, deletedBy), Transaction);
            return result is null
                ? Result<RedblockEntity>.Failure("Redblock not found.", HttpStatusCode.NotFound)
                : Result<RedblockEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockEntity>.Failure($"Failed to soft delete redblock: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<RedblockStatusEntity>> InsertStatus(long projectId, long keyNumber, string status, long createdBy)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.InsertStatus, (projectId, keyNumber, status, createdBy), Transaction);
            return result is null
                ? Result<RedblockStatusEntity>.Failure("Failed to insert redblock status: No status returned from database.", HttpStatusCode.InternalServerError)
                : Result<RedblockStatusEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockStatusEntity>.Failure($"Failed to insert redblock status: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<RedblockUserAssignmentEntity>> InsertUserAssignment(long projectId, long keyNumber, long assignedTo, long createdBy)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.InsertUserAssignment, (projectId, keyNumber, assignedTo, createdBy), Transaction);
            return result is null
                ? Result<RedblockUserAssignmentEntity>.Failure("Failed to insert redblock user assignment: No assignment returned from database.", HttpStatusCode.InternalServerError)
                : Result<RedblockUserAssignmentEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockUserAssignmentEntity>.Failure($"Failed to insert redblock user assignment: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result> DeleteUserAssignment(long projectId, long keyNumber, long assignedTo)
    {
        try
        {
            var rows = await Connection.ExecuteProcedure(StoredProcs.Redblocks.DeleteUserAssignment, (projectId, keyNumber, assignedTo), Transaction);
            return rows > 0
                ? Result.Success()
                : Result.Failure("No user assignment was deleted.", HttpStatusCode.NotFound);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to delete redblock user assignment: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<RedblockRoleAssignmentEntity>> InsertRoleAssignment(long projectId, long keyNumber, string roleName, long createdBy)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.InsertRoleAssignment, (projectId, keyNumber, roleName, createdBy), Transaction);
            return result is null
                ? Result<RedblockRoleAssignmentEntity>.Failure("Failed to insert redblock role assignment: No assignment returned from database.", HttpStatusCode.InternalServerError)
                : Result<RedblockRoleAssignmentEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockRoleAssignmentEntity>.Failure($"Failed to insert redblock role assignment: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result> DeleteRoleAssignment(long projectId, long keyNumber, string roleName)
    {
        try
        {
            var rows = await Connection.ExecuteProcedure(StoredProcs.Redblocks.DeleteRoleAssignment, (projectId, keyNumber, roleName), Transaction);
            return rows > 0
                ? Result.Success()
                : Result.Failure("No role assignment was deleted.", HttpStatusCode.NotFound);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to delete redblock role assignment: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}
