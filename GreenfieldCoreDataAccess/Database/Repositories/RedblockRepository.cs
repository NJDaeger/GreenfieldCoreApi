using System.Data;
using System.Data.Common;
using System.Net;
using Dapper;
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

    public async Task<Result<RedblockProjectEntity>> SelectProjectByKey(string projectKey)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.SelectProjectByKey, projectKey, Transaction);
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

    public async Task<Result<RedblockEntity>> SelectRedblockById(long redblockId)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.SelectRedblockById, redblockId, Transaction);
            return result is null
                ? Result<RedblockEntity>.Failure("Redblock not found.", HttpStatusCode.NotFound)
                : Result<RedblockEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockEntity>.Failure($"Failed to select redblock by ID: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<(IEnumerable<RedblockEntity> redblocks, bool hasMore, long? nextCursor)>> SelectRedblocksByProject(
        long projectId,
        string? statusFilter,
        string? statusFilterMatchType,
        string? deletionFilter,
        string? deletionFilterMatchType,
        string? userAssignmentFilter,
        string? userAssignmentFilterMatchType,
        string? roleAssignmentFilter,
        string? roleAssignmentFilterMatchType,
        string? messageFilter,
        string? messageFilterMatchType,
        int pageSize,
        long? searchAfterRedblockId)
    {
        try
        {
            // Clamp page size to reasonable limits
            var actualPageSize = Math.Max(1, Math.Min(pageSize, 500));
            var fetchCount = actualPageSize + 1; // Fetch one extra to detect HasMore

            var sql = BuildRedblockSearchQuery(
                statusFilter, statusFilterMatchType,
                deletionFilter, deletionFilterMatchType,
                userAssignmentFilter, userAssignmentFilterMatchType,
                roleAssignmentFilter, roleAssignmentFilterMatchType,
                messageFilter, messageFilterMatchType,
                searchAfterRedblockId,
                fetchCount);

            var parameters = new DynamicParameters();
            parameters.Add("@ProjectId", projectId, DbType.Int64);
            parameters.Add("@PageSize", fetchCount, DbType.Int32);

            // Add filter parameters only if they're provided
            if (!string.IsNullOrEmpty(statusFilter))
                parameters.Add("@StatusFilter", statusFilter, DbType.String);
            if (!string.IsNullOrEmpty(deletionFilter))
                parameters.Add("@DeletionFilter", deletionFilter, DbType.String);
            if (!string.IsNullOrEmpty(userAssignmentFilter))
                parameters.Add("@UserAssignmentFilter", userAssignmentFilter, DbType.String);
            if (!string.IsNullOrEmpty(roleAssignmentFilter))
                parameters.Add("@RoleAssignmentFilter", roleAssignmentFilter, DbType.String);
            if (!string.IsNullOrEmpty(messageFilter))
                parameters.Add("@MessageFilter", messageFilter, DbType.String);
            if (searchAfterRedblockId.HasValue)
                parameters.Add("@SearchAfterRedblockId", searchAfterRedblockId.Value, DbType.Int64);

            var results = await Connection.QueryAsync<RedblockEntity>(sql, parameters, Transaction);
            var resultList = results.ToList();

            var hasMore = resultList.Count > actualPageSize;
            var returnedResults = hasMore ? resultList.Take(actualPageSize).ToList() : resultList;
            var nextCursor = hasMore ? returnedResults.LastOrDefault()?.RedblockId : null;

            return Result<(IEnumerable<RedblockEntity>, bool, long?)>.Success(
                (returnedResults, hasMore, nextCursor));
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<(IEnumerable<RedblockEntity>, bool, long?)>.Failure(
                $"Failed to select redblocks by project: {ex.Message}",
                HttpStatusCode.InternalServerError);
        }
    }

    private string BuildRedblockSearchQuery(
        string? statusFilter, string? statusFilterMatchType,
        string? deletionFilter, string? deletionFilterMatchType,
        string? userAssignmentFilter, string? userAssignmentFilterMatchType,
        string? roleAssignmentFilter, string? roleAssignmentFilterMatchType,
        string? messageFilter, string? messageFilterMatchType,
        long? searchAfterRedblockId,
        int fetchCount)
    {
        // Build dynamic WHERE clauses based on provided filters
        var whereClauses = new List<string> { "rb.ProjectId = @ProjectId" };

        if (searchAfterRedblockId.HasValue)
            whereClauses.Add("rb.RedblockId > @SearchAfterRedblockId");

        // Status filter
        if (!string.IsNullOrEmpty(statusFilter) && !string.IsNullOrEmpty(statusFilterMatchType))
        {
            whereClauses.Add(BuildStatusFilterWhereClause(statusFilterMatchType));
        }

        // Deletion filter
        if (!string.IsNullOrEmpty(deletionFilter) && !string.IsNullOrEmpty(deletionFilterMatchType))
        {
            whereClauses.Add(BuildDeletionFilterWhereClause(deletionFilterMatchType));
        }

        // User assignment filter
        if (!string.IsNullOrEmpty(userAssignmentFilter) && !string.IsNullOrEmpty(userAssignmentFilterMatchType))
        {
            whereClauses.Add(BuildUserAssignmentFilterWhereClause(userAssignmentFilterMatchType));
        }

        // Role assignment filter
        if (!string.IsNullOrEmpty(roleAssignmentFilter) && !string.IsNullOrEmpty(roleAssignmentFilterMatchType))
        {
            whereClauses.Add(BuildRoleAssignmentFilterWhereClause(roleAssignmentFilterMatchType));
        }

        // Message filter
        if (!string.IsNullOrEmpty(messageFilter) && !string.IsNullOrEmpty(messageFilterMatchType))
        {
            whereClauses.Add(BuildMessageFilterWhereClause(messageFilterMatchType));
        }

        var whereClause = string.Join(" AND ", whereClauses);

        var sql = $@"
            SELECT
                rb.RedblockId,
                rb.ProjectId,
                rb.KeyNumber,
                rb.Message,
                rb.X,
                rb.Y,
                rb.Z,
                rb.CreatedBy,
                rb.CreatedOn,
                rb.DeletedBy,
                rb.DeletedOn
            FROM `Redblocks.Redblocks` rb
            WHERE {whereClause}
            ORDER BY rb.RedblockId ASC
            LIMIT @PageSize;
        ";

        return sql;
    }

    private string BuildStatusFilterWhereClause(string matchType)
    {
        if (matchType == "or")
        {
            return """
                   rb.RedblockId IN (
                       SELECT rs.RedblockId
                       FROM `Redblocks.Statuses` rs
                       WHERE rs.RedblockId = rb.RedblockId
                         AND rs.StatusId = (
                           SELECT MAX(s2.StatusId)
                           FROM `Redblocks.Statuses` s2
                           WHERE s2.RedblockId = rs.RedblockId
                         )
                         AND JSON_CONTAINS(@StatusFilter, JSON_QUOTE(rs.Status))
                   )
                   """;
        }
        else if (matchType == "not")
        {
            return """
                   (rb.RedblockId NOT IN (
                       SELECT rs.RedblockId
                       FROM `Redblocks.Statuses` rs
                       WHERE rs.RedblockId = rb.RedblockId
                         AND rs.StatusId = (
                           SELECT MAX(s2.StatusId)
                           FROM `Redblocks.Statuses` s2
                           WHERE s2.RedblockId = rs.RedblockId
                         )
                         AND JSON_CONTAINS(@StatusFilter, JSON_QUOTE(rs.Status))
                   )
                   OR rb.RedblockId NOT IN (
                       SELECT DISTINCT RedblockId
                       FROM `Redblocks.Statuses`
                   ))
                   """;
        }
        return "";
    }

    private string BuildDeletionFilterWhereClause(string matchType)
    {
        if (matchType == "or")
            return "rb.DeletedBy IN (SELECT JSON_UNQUOTE(JSON_EXTRACT(@DeletionFilter, CONCAT('$[', idx, ']'))) FROM (SELECT 0 AS idx UNION SELECT 1 UNION SELECT 2 UNION SELECT 3 UNION SELECT 4) AS t WHERE JSON_EXTRACT(@DeletionFilter, CONCAT('$[', idx, ']')) IS NOT NULL)";
        else if (matchType == "not")
            return "(rb.DeletedBy IS NULL OR rb.DeletedBy NOT IN (SELECT JSON_UNQUOTE(JSON_EXTRACT(@DeletionFilter, CONCAT('$[', idx, ']'))) FROM (SELECT 0 AS idx UNION SELECT 1 UNION SELECT 2 UNION SELECT 3 UNION SELECT 4) AS t WHERE JSON_EXTRACT(@DeletionFilter, CONCAT('$[', idx, ']')) IS NOT NULL))";
        return "";
    }

    private string BuildUserAssignmentFilterWhereClause(string matchType)
    {
        if (matchType == "or")
            return "rb.RedblockId IN (SELECT DISTINCT ua.RedblockId FROM `Redblocks.UserAssignments` ua WHERE JSON_CONTAINS(@UserAssignmentFilter, JSON_QUOTE(CAST(ua.AssignedTo AS CHAR))))";
        else if (matchType == "not")
            return "rb.RedblockId NOT IN (SELECT DISTINCT ua.RedblockId FROM `Redblocks.UserAssignments` ua WHERE JSON_CONTAINS(@UserAssignmentFilter, JSON_QUOTE(CAST(ua.AssignedTo AS CHAR))))";
        return "";
    }

    private string BuildRoleAssignmentFilterWhereClause(string matchType)
    {
        if (matchType == "or")
            return "rb.RedblockId IN (SELECT DISTINCT ra.RedblockId FROM `Redblocks.RoleAssignments` ra WHERE JSON_CONTAINS(@RoleAssignmentFilter, JSON_QUOTE(ra.RoleName)))";
        else if (matchType == "not")
            return "rb.RedblockId NOT IN (SELECT DISTINCT ra.RedblockId FROM `Redblocks.RoleAssignments` ra WHERE JSON_CONTAINS(@RoleAssignmentFilter, JSON_QUOTE(ra.RoleName)))";
        return "";
    }

    private string BuildMessageFilterWhereClause(string matchType)
    {
        if (matchType == "contains")
            return "rb.Message LIKE CONCAT('%', @MessageFilter, '%')";
        else if (matchType == "exact")
            return "rb.Message = @MessageFilter";
        else if (matchType == "startsWith")
            return "rb.Message LIKE CONCAT(@MessageFilter, '%')";
        else if (matchType == "endsWith")
            return "rb.Message LIKE CONCAT('%', @MessageFilter)";
        return "";
    }

    public async Task<Result<IEnumerable<RedblockStatusEntity>>> SelectRedblockStatuses(long redblockId)
    {
        try
        {
            var result = await Connection.QueryProcedure(StoredProcs.Redblocks.SelectRedblockStatuses, redblockId, Transaction);
            return Result<IEnumerable<RedblockStatusEntity>>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<IEnumerable<RedblockStatusEntity>>.Failure($"Failed to select redblock statuses: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<RedblockUserAssignmentEntity>>> SelectRedblockUserAssignments(long redblockId)
    {
        try
        {
            var result = await Connection.QueryProcedure(StoredProcs.Redblocks.SelectRedblockUserAssignments, redblockId, Transaction);
            return Result<IEnumerable<RedblockUserAssignmentEntity>>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<IEnumerable<RedblockUserAssignmentEntity>>.Failure($"Failed to select redblock user assignments: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<RedblockRoleAssignmentEntity>>> SelectRedblockRoleAssignments(long redblockId)
    {
        try
        {
            var result = await Connection.QueryProcedure(StoredProcs.Redblocks.SelectRedblockRoleAssignments, redblockId, Transaction);
            return Result<IEnumerable<RedblockRoleAssignmentEntity>>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<IEnumerable<RedblockRoleAssignmentEntity>>.Failure($"Failed to select redblock role assignments: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<RedblockEntity>> UpdateRedblockMessage(long projectId, long keyNumber, string message, long updatedBy)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.UpdateRedblockMessage, (projectId, keyNumber, message, updatedBy), Transaction);
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
