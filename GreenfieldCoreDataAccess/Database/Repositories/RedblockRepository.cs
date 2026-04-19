using System.Data;
using System.Data.Common;
using System.Net;
using Dapper;
using GreenfieldCoreDataAccess.Database.Helpers;
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

    public async Task<Result<RedblockWithLatestStatusEntity>> SelectRedblockByKey(long projectId, long keyNumber)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.SelectRedblockByKey, (projectId, keyNumber), Transaction);
            return result is null
                ? Result<RedblockWithLatestStatusEntity>.Failure("Redblock not found.", HttpStatusCode.NotFound)
                : Result<RedblockWithLatestStatusEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockWithLatestStatusEntity>.Failure($"Failed to select redblock: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<(IEnumerable<RedblockWithLatestStatusEntity> Redblocks, long TotalResults)>>
        SelectRedblocksByProject(long projectId,
            Location? location,
            long? radius,
            List<string> statuses, string? statusFilterMatchType,
            List<long> deletionUserIds, string? deletionFilterMatchType,
            List<long> userAssignmentUserIds, string? userAssignmentFilterMatchType,
            List<string> roleAssignmentRoleNames, string? roleAssignmentFilterMatchType,
            string messageFilter, string? messageFilterMatchType,
            int resultsPerPage, long currentPage)
    {
        try
        {
            var statementBuilder = StatementBuilder
                .SelectFrom("`Redblocks.Redblocks` rb")
                .Columns("""
                         rb.RedblockId
                         ,rb.ProjectId 
                         ,rb.KeyNumber
                         ,rb.Message
                         ,rs_ranked.Status
                         ,rb.X
                         ,rb.Y
                         ,rb.Z
                         ,rb.CreatedBy
                         ,rb.CreatedOn
                         ,rb.UpdatedBy
                         ,rb.UpdatedOn
                         ,rb.DeletedBy
                         ,rb.DeletedOn
                         """)
                .Join("""
                        inner join (
                          select rs.*, ROW_NUMBER() over (partition by rs.RedblockId order by rs.StatusId desc) as StatusNumber
                          from `Redblocks.Statuses` rs
                        ) rs_ranked on rb.RedblockId = rs_ranked.RedblockId and rs_ranked.StatusNumber = 1
                      """)
                .WithParameter("@ProjectId", projectId)
                .Where("rb.ProjectId = @ProjectId")
                .WithLimit(resultsPerPage)
                .WithOffset(resultsPerPage * (currentPage - 1));

            var statusStatementPart = BuildStatusStatementParts(statuses, statusFilterMatchType);
            if (statusStatementPart != null) statementBuilder.WithPart(statusStatementPart);

            var deletionStatementPart = BuildDeletionStatementParts(deletionUserIds, deletionFilterMatchType);
            if (deletionStatementPart != null) statementBuilder.WithPart(deletionStatementPart);

            var userAssignmentStatementPart = BuildUserAssignmentStatementParts(userAssignmentUserIds, userAssignmentFilterMatchType);
            if (userAssignmentStatementPart != null) statementBuilder.WithPart(userAssignmentStatementPart);

            var roleAssignmentStatementPart = BuildRoleAssignmentStatementParts(roleAssignmentRoleNames, roleAssignmentFilterMatchType);
            if (roleAssignmentStatementPart != null) statementBuilder.WithPart(roleAssignmentStatementPart);

            var messageStatementPart = BuildMessageStatementParts(messageFilter, messageFilterMatchType);
            if (messageStatementPart != null) statementBuilder.WithPart(messageStatementPart);

            var spatialStatementPart = BuildSpatialStatementParts(location, radius);
            if (spatialStatementPart != null) statementBuilder.WithPart(spatialStatementPart);

            var countStatement = statementBuilder.BuildCount();
            var totalResults = await Connection.ExecuteScalarAsync<long>(countStatement.query, countStatement.parameters, Transaction);
            
            var statement = statementBuilder.Build();
            var redblocks = (await Connection.QueryAsync<RedblockWithLatestStatusEntity>(statement.query, statement.parameters, Transaction)).ToList();
            
            return Result<(IEnumerable<RedblockWithLatestStatusEntity> Redblocks, long TotalResults)>.Success((redblocks, totalResults));
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<(IEnumerable<RedblockWithLatestStatusEntity> Redblocks, long TotalResults)>.Failure($"Failed to select redblocks: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    #region Query String Builders

    private static StatementPart? BuildSpatialStatementParts(Location? location, long? radius)
    {
        if (location == null) return null;
        var statementPart = new StatementPartBuilder()
            .WithParameter("@LocationX", location.Value.X)
            .WithParameter("@LocationY", location.Value.Y)
            .WithParameter("@LocationZ", location.Value.Z)
            .Columns("POW(rb.X - @LocationX, 2) + POW(rb.Y - @LocationY, 2) + POW(rb.Z - @LocationZ, 2) AS DistanceSquared")
            .OrderBy("DistanceSquared ASC");

        if (radius is null) return statementPart.Build();

        statementPart.WithParameter("@Radius", radius)
            .WithParameter("@RadiusSquared", radius.Value * radius.Value)
            .Having("DistanceSquared <= @RadiusSquared");

        return statementPart.Build();
    }
    
    private static StatementPart? BuildStatusStatementParts(List<string> statuses, string? matchType)
    {
        if (string.IsNullOrWhiteSpace(matchType)) return null;
        var builder = new StatementPartBuilder();
        
        foreach (var status in statuses) 
            builder.WithIndexedParameter("@Status", status);

        var inClause = builder.Build().JoinParameterKeys();
        
        if (matchType.Equals("or", StringComparison.OrdinalIgnoreCase))
            return statuses.Count == 0 
                ? null
                : builder.Where($"AND (rs_ranked.Status IS NULL OR rs_ranked.Status IN ({inClause}))").Build();
        
        if (matchType.Equals("not", StringComparison.OrdinalIgnoreCase))
            return statuses.Count == 0 
                ? builder.Where("AND (rs_ranked.Status IS NOT NULL)").Build() 
                : builder.Where($"AND (rs_ranked.Status NOT IN ({inClause}) OR rs_ranked.Status IS NULL)").Build();
        
        return null;
    }

    private static StatementPart? BuildDeletionStatementParts(List<long>  deletionUserIds, string? matchType)
    {
        if (string.IsNullOrWhiteSpace(matchType)) return null;
        var builder = new StatementPartBuilder();
        
        foreach (var deletionUserId in deletionUserIds) 
            builder.WithIndexedParameter("@DeletionUserId", deletionUserId);
        
        var inClause = builder.Build().JoinParameterKeys();
        
        if (matchType.Equals("or", StringComparison.OrdinalIgnoreCase)) 
            return deletionUserIds.Count == 0
                ? null //if there are no user IDs to match with "or", then we don't need to filter by deletion user at all since "or" with no values should include everything
                : builder.Where($"AND (rb.DeletedBy IN ({inClause})) OR rb.DeletedBy IS NULL").Build();
        
        if (matchType.Equals("not", StringComparison.OrdinalIgnoreCase))
            return deletionUserIds.Count == 0
                ? builder.Where("AND (rb.DeletedBy IS NOT NULL)").Build()
                : builder.Where($"AND (rb.DeletedBy NOT IN ({inClause}) OR rb.DeletedBy IS NULL)").Build();

        if (matchType.Equals("and", StringComparison.OrdinalIgnoreCase))
            return deletionUserIds.Count == 0
                ? builder.Where("AND rb.DeletedBy IS NULL").Build()
                : null; //redblocks can only have one deletion user, so "and" with multiple user IDs will not 
        
        return null;
    }

    private static StatementPart? BuildUserAssignmentStatementParts(List<long> userIds, string? matchType)
    {
        if (string.IsNullOrWhiteSpace(matchType)) return null;
        var builder = new StatementPartBuilder();
        
        foreach (var userId in userIds)
            builder.WithIndexedParameter("@UserId", userId);
        
        var inClause = builder.Build().JoinParameterKeys();
        
        // or + [] = return regardless of assignment status
        // or + [...] = return redblocks with at least one of the specified user assignments
        if (matchType.Equals("or", StringComparison.OrdinalIgnoreCase))
            return userIds.Count == 0
                ? null
                : builder.Join($"INNER JOIN `Redblocks.UserAssignments` rua ON rb.RedblockId = rua.RedblockId AND rua.AssignedTo IN ({inClause})").Build();

        // not + [] = return only redblocks with user assignments ("not empty")
        // not + [...] = return redblocks with no user assignments or user assignments that do not include any of the specified users
        if (matchType.Equals("not", StringComparison.OrdinalIgnoreCase))
            return userIds.Count == 0
                ? builder.Join("INNER JOIN `Redblocks.UserAssignments` rua ON rb.RedblockId = rua.RedblockId").Build()
                : builder.Join($"LEFT JOIN `Redblocks.UserAssignments` rua ON rb.RedblockId = rua.RedblockId")
                    .Where($"AND (rua.AssignedTo NOT IN ({inClause}) OR rua.AssignedTo IS NULL)")
                    .Build();
        
        // and + [] = return redblocks with no users assigned ("empty")
        // and + [...] = return redblocks that have all the specified users assigned
        if  (matchType.Equals("and", StringComparison.OrdinalIgnoreCase))
            return userIds.Count == 0
                ? builder.Join("LEFT JOIN `Redblocks.UserAssignments` rua ON rb.RedblockId = rua.RedblockId")
                    .Where("AND rua.AssignedTo IS NULL")
                    .Build()
                : builder.Join($"INNER JOIN (SELECT RedblockId FROM `Redblocks.UserAssignments` WHERE AssignedTo IN ({inClause}) GROUP BY RedblockId HAVING COUNT(DISTINCT AssignedTo) = {userIds.Count}) rua ON rb.RedblockId = rua.RedblockId").Build();
        
        return null;
    }

    private static StatementPart? BuildRoleAssignmentStatementParts(List<string> roles, string? matchType)
    {
        if (string.IsNullOrWhiteSpace(matchType)) return null;
        var builder = new StatementPartBuilder();
        
        foreach (var role in roles)
            builder.WithIndexedParameter("@Role", role);
        
        var inClause = builder.Build().JoinParameterKeys();
        
        // or + [] = return regardless of role assignment status
        // or + [...] = return redblocks with at least one of the specified role assignments
        if (matchType.Equals("or", StringComparison.OrdinalIgnoreCase))
            return roles.Count == 0
                ? null
                : builder.Join($"INNER JOIN `Redblocks.RoleAssignments` rra ON rb.RedblockId = rra.RedblockId AND rra.Role IN ({inClause})").Build();

        // not + [] = return only redblocks with role assignments ("not empty")
        // not + [...] = return redblocks with no role assignments or role assignments that do not include any of the specified roles
        if (matchType.Equals("not", StringComparison.OrdinalIgnoreCase))
            return roles.Count == 0
                ? builder.Join("INNER JOIN `Redblocks.RoleAssignments` rra ON rb.RedblockId = rra.RedblockId").Build()
                : builder.Join("LEFT JOIN `Redblocks.RoleAssignments` rra ON rb.RedblockId = rra.RedblockId")
                    .Where($"AND (rra.Role NOT IN ({inClause}) OR rra.Role IS NULL)")
                    .Build();

        // and + [] = return redblocks with no roles assigned ("empty")
        // and + [...] = return redblocks that have all the specified roles assigned
        if  (matchType.Equals("and", StringComparison.OrdinalIgnoreCase))
            return roles.Count == 0
                ? builder.Join("LEFT JOIN `Redblocks.RoleAssignments` rra ON rb.RedblockId = rra.RedblockId")
                    .Where("AND rra.Role IS NULL")
                    .Build()
                : builder.Join($"INNER JOIN (SELECT RedblockId FROM `Redblocks.RoleAssignments` WHERE Role IN ({inClause}) GROUP BY RedblockId HAVING COUNT(DISTINCT Role) = {roles.Count}) rra ON rb.RedblockId = rra.RedblockId").Build();
        
        return null;
    }

    private static StatementPart? BuildMessageStatementParts(string searchText, string? matchType)
    {
        if (string.IsNullOrWhiteSpace(matchType)) return null;
        var builder = new StatementPartBuilder().WithParameter("@SearchText", searchText);
        
        if (matchType.Equals("contains", StringComparison.OrdinalIgnoreCase))
            return builder.Where($"AND rb.Message LIKE CONCAT('%', @SearchText, '%')").Build();
        
        if (matchType.Equals("startsWith", StringComparison.OrdinalIgnoreCase))
            return builder.Where($"AND rb.Message LIKE CONCAT(@SearchText, '%')").Build();
        
        if (matchType.Equals("endsWith", StringComparison.OrdinalIgnoreCase))
            return builder.Where($"AND rb.Message LIKE CONCAT('%', @SearchText)").Build();
        
        if (matchType.Equals("exact", StringComparison.OrdinalIgnoreCase))
            return builder.Where($"AND rb.Message = @SearchText").Build();
        
        return null;
    }
    
    #endregion
    
    public async Task<Result<RedblockWithLatestStatusEntity>> SelectRedblockById(long redblockId)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.SelectRedblockById, redblockId, Transaction);
            return result is null
                ? Result<RedblockWithLatestStatusEntity>.Failure("Redblock not found.", HttpStatusCode.NotFound)
                : Result<RedblockWithLatestStatusEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockWithLatestStatusEntity>.Failure($"Failed to select redblock by ID: {ex.Message}", HttpStatusCode.InternalServerError);
        }
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

    public async Task<Result<IEnumerable<Guid>>> SelectRedblockEntities(long redblockId)
    {
        try
        {
            var result = await Connection.QueryProcedure(StoredProcs.Redblocks.SelectRedblockEntities, redblockId, Transaction);
            return Result<IEnumerable<Guid>>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<IEnumerable<Guid>>.Failure($"Failed to select redblock entities: {ex.Message}", HttpStatusCode.InternalServerError);
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

    public async Task<Result> InsertRedblockEntity(long projectId, long keyNumber, Guid entityGuid)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.InsertRedblockEntity, (projectId, keyNumber, entityGuid), Transaction);
            return result is null
                ? Result.Failure("Failed to insert redblock entity mapping: No mapping returned from database.", HttpStatusCode.InternalServerError)
                : Result.Success();
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to insert redblock entity mapping: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result> DeleteRedblockEntities(long projectId, long keyNumber)
    {
        try
        {
            await Connection.ExecuteProcedure(StoredProcs.Redblocks.DeleteRedblockEntities, (projectId, keyNumber), Transaction);
            return Result.Success();
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to delete redblock entities: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}
