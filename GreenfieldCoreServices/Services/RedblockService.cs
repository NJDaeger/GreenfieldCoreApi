using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Redblocks;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Services;

public class RedblockService(IUnitOfWork uow, ILogger<IRedblockService> logger) : IRedblockService
{
    public async Task<Result<Redblock>> CreateRedblock(long projectId, int x, int y, int z, string message, long createdBy, string initialStatus, List<long> assignedUsers, List<string> assignedRoles)
    {
        if (string.IsNullOrWhiteSpace(initialStatus))
            return Result<Redblock>.Failure("Initial status cannot be empty.");
        
        if (string.IsNullOrWhiteSpace(message))
            return Result<Redblock>.Failure("Message cannot be empty.");
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();

        var insertResult = await redblockRepo.InsertRedblock(projectId, message, x, y, z, createdBy);
        
        if (!insertResult.TryGetDataNonNull(out var redblockEntity))
            return Result<Redblock>.Failure(insertResult.ErrorMessage ?? "Failed to create redblock.", insertResult.StatusCode);
        
        var insertStatus = await redblockRepo.InsertStatus(projectId, redblockEntity.KeyNumber, initialStatus, createdBy);
        if (!insertStatus.TryGetDataNonNull(out var statusEntity))
            return Result<Redblock>.Failure(insertStatus.ErrorMessage ?? "Failed to set initial status on redblock.", insertStatus.StatusCode);

        var actualAssignedUsers = new List<RedblockUserAssignment>();
        var actualAssignedRoles = new List<RedblockRoleAssignment>();
        
        foreach (var userId in assignedUsers)
        {
            var insertUserResult = await redblockRepo.InsertUserAssignment(projectId, redblockEntity.KeyNumber, userId, createdBy);
            if (!insertUserResult.TryGetDataNonNull(out var assignedUser))
                logger.LogWarning("Failed to assign user {UserId} to redblock {RedblockId} in project {ProjectId}: {ErrorMessage}", userId, redblockEntity.RedblockId, projectId, insertUserResult.ErrorMessage);
            else
                actualAssignedUsers.Add(RedblockUserAssignment.FromModel(assignedUser));
        }

        foreach (var roleName in assignedRoles)
        {
            var insertRoleResult = await redblockRepo.InsertRoleAssignment(projectId, redblockEntity.KeyNumber, roleName, createdBy);
            if (!insertRoleResult.TryGetDataNonNull(out var assignedRole))
                logger.LogWarning("Failed to assign role {RoleName} to redblock {RedblockId} in project {ProjectId}: {ErrorMessage}", roleName, redblockEntity.RedblockId, projectId, insertRoleResult.ErrorMessage);
            else
                actualAssignedRoles.Add(RedblockRoleAssignment.FromModel(assignedRole));
        }

        uow.CompleteAndCommit();
        
        var mappedRedblock = Redblock.FromModel(redblockEntity);
        mappedRedblock.RoleAssignments = actualAssignedRoles;
        mappedRedblock.UserAssignments = actualAssignedUsers;
        mappedRedblock.Statuses = [RedblockStatus.FromModel(statusEntity)];
        
        return Result<Redblock>.Success(mappedRedblock);
    }

    public async Task<Result> DeleteRedblock(long projectId, long keyNumber, long deletedBy)
    {
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var deletionResult = await redblockRepo.SoftDeleteRedblock(projectId, keyNumber, deletedBy);
        if (!deletionResult.IsSuccessful)
            return Result.Failure(deletionResult.ErrorMessage ?? "Failed to delete redblock.", deletionResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        return Result.Success();
    }

    public async Task<Result> UpdateRedblock(long projectId, long keyNumber, string newMessage, long updatedBy)
    {
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var updateResult = await redblockRepo.UpdateRedblockMessage(projectId, keyNumber, newMessage, updatedBy);
        if (!updateResult.IsSuccessful)
            return Result.Failure(updateResult.ErrorMessage ?? "Failed to update redblock message.", updateResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        return Result.Success();
    }

    public async Task<Result<Redblock>> GetRedblockById(long redblockId)
    {
        var redblockRepo = uow.Repository<IRedblockRepository>();
        
        var redblockResult = await redblockRepo.SelectRedblockById(redblockId);
        if (!redblockResult.TryGetDataNonNull(out var redblockEntity))
            return Result<Redblock>.Failure(redblockResult.ErrorMessage ?? "Failed to retrieve redblock.", redblockResult.StatusCode);

        var statusesResult = await redblockRepo.SelectRedblockStatuses(redblockId);
        if (!statusesResult.TryGetDataNonNull(out var statusEntities))
            return Result<Redblock>.Failure(statusesResult.ErrorMessage ?? "Failed to retrieve redblock statuses.", statusesResult.StatusCode);

        var userAssignmentsResult = await redblockRepo.SelectRedblockUserAssignments(redblockId);
        if (!userAssignmentsResult.TryGetDataNonNull(out var userAssignmentEntities))
            return Result<Redblock>.Failure(userAssignmentsResult.ErrorMessage ?? "Failed to retrieve redblock user assignments.", userAssignmentsResult.StatusCode);

        var roleAssignmentsResult = await redblockRepo.SelectRedblockRoleAssignments(redblockId);
        if (!roleAssignmentsResult.TryGetDataNonNull(out var roleAssignmentEntities))
            return Result<Redblock>.Failure(roleAssignmentsResult.ErrorMessage ?? "Failed to retrieve redblock role assignments.", roleAssignmentsResult.StatusCode);

        var mappedRedblock = Redblock.FromModel(redblockEntity);
        mappedRedblock.Statuses = statusEntities.Select(RedblockStatus.FromModel).ToList();
        mappedRedblock.UserAssignments = userAssignmentEntities.Select(RedblockUserAssignment.FromModel).ToList();
        mappedRedblock.RoleAssignments = roleAssignmentEntities.Select(RedblockRoleAssignment.FromModel).ToList();
        
        return Result<Redblock>.Success(mappedRedblock);
        
    }

    public async Task<Result<Redblock>> GetRedblockByKey(long projectId, long keyNumber)
    {
        var redblockRepo = uow.Repository<IRedblockRepository>();
        
        var redblockResult = await redblockRepo.SelectRedblockByKey(projectId, keyNumber);
        if (!redblockResult.TryGetDataNonNull(out var redblockEntity))
            return Result<Redblock>.Failure(redblockResult.ErrorMessage ?? "Failed to retrieve redblock.", redblockResult.StatusCode);
        
        var statusesResult = await redblockRepo.SelectRedblockStatuses(redblockEntity.RedblockId);
        if (!statusesResult.TryGetDataNonNull(out var statusEntities))
            return Result<Redblock>.Failure(statusesResult.ErrorMessage ?? "Failed to retrieve redblock statuses.", statusesResult.StatusCode);

        var userAssignmentsResult = await redblockRepo.SelectRedblockUserAssignments(redblockEntity.RedblockId);
        if (!userAssignmentsResult.TryGetDataNonNull(out var userAssignmentEntities))
            return Result<Redblock>.Failure(userAssignmentsResult.ErrorMessage ?? "Failed to retrieve redblock user assignments.", userAssignmentsResult.StatusCode);

        var roleAssignmentsResult = await redblockRepo.SelectRedblockRoleAssignments(redblockEntity.RedblockId);
        if (!roleAssignmentsResult.TryGetDataNonNull(out var roleAssignmentEntities))
            return Result<Redblock>.Failure(roleAssignmentsResult.ErrorMessage ?? "Failed to retrieve redblock role assignments.", roleAssignmentsResult.StatusCode);

        var mappedRedblock = Redblock.FromModel(redblockEntity);
        mappedRedblock.Statuses = statusEntities.Select(RedblockStatus.FromModel).ToList();
        mappedRedblock.UserAssignments = userAssignmentEntities.Select(RedblockUserAssignment.FromModel).ToList();
        mappedRedblock.RoleAssignments = roleAssignmentEntities.Select(RedblockRoleAssignment.FromModel).ToList();

        return Result<Redblock>.Success(mappedRedblock);
    }

    public async Task<Result<RedblockSearchResult>> GetRedblocksByProject(long projectId, RedblockSearchRequest searchFilter)
    {
        var redblockRepo = uow.Repository<IRedblockRepository>();

        // Validate and clamp page size
        var pageSize = Math.Max(1, Math.Min(searchFilter.PageSize, 500));

        // Build JSON filter strings (only if filter is provided and has items)
        var statusFilter = searchFilter.StatusFilter?.Statuses.Count > 0
            ? $"[{string.Join(",", searchFilter.StatusFilter.Statuses.Select(s => $"\"{s}\""))}]"
            : null;
        var statusFilterMatchType = statusFilter == null ? null : searchFilter.StatusFilter!.MatchType;

        var deletionFilter = searchFilter.DeletionFilter?.Users.Count > 0
            ? $"[{string.Join(",", searchFilter.DeletionFilter.Users)}]"
            : null;
        var deletionFilterMatchType = deletionFilter == null ? null : searchFilter.DeletionFilter!.MatchType;

        var userAssignmentFilter = searchFilter.UserAssignmentFilter?.Users.Count > 0
            ? $"[{string.Join(",", searchFilter.UserAssignmentFilter.Users)}]"
            : null;
        var userAssignmentFilterMatchType = userAssignmentFilter == null ? null : searchFilter.UserAssignmentFilter!.MatchType;

        var roleAssignmentFilter = searchFilter.RoleAssignmentFilter?.Roles.Count > 0
            ? $"[{string.Join(",", searchFilter.RoleAssignmentFilter.Roles.Select(r => $"\"{r}\""))}]"
            : null;
        var roleAssignmentFilterMatchType = roleAssignmentFilter == null ? null : searchFilter.RoleAssignmentFilter!.MatchType;

        var messageFilter = searchFilter.MessageFilter?.Content;
        var messageFilterMatchType = messageFilter == null ? null : searchFilter.MessageFilter!.MatchType;

        // Call repository to get search results with pagination
        var redblocksResult = await redblockRepo.SelectRedblocksByProject(
            projectId,
            statusFilter, statusFilterMatchType,
            deletionFilter, deletionFilterMatchType,
            userAssignmentFilter, userAssignmentFilterMatchType,
            roleAssignmentFilter, roleAssignmentFilterMatchType,
            messageFilter, messageFilterMatchType,
            pageSize,
            searchFilter.SearchAfterRedblockId);

        if (!redblocksResult.IsSuccessful)
            return Result<RedblockSearchResult>.Failure(
                redblocksResult.ErrorMessage ?? "Failed to retrieve redblocks for project.",
                redblocksResult.StatusCode);

        var (redblockEntities, hasMore, nextCursor) = redblocksResult.GetNonNullOrThrow();
        var redblockList = redblockEntities.ToList();

        // Get the project details to include in identifiers
        var projectResult = await redblockRepo.SelectProjectById(projectId);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockSearchResult>.Failure(
                projectResult.ErrorMessage ?? "Failed to retrieve project for search results.",
                projectResult.StatusCode);

        // Map to search identifiers (lightweight results)
        var searchIdentifiers = redblockList
            .Select(rb => new RedblockSearchIdentifier
            {
                RedblockId = rb.RedblockId,
                ProjectKey = projectEntity.ProjectKey,
                KeyNumber = rb.KeyNumber
            })
            .ToList();

        return Result<RedblockSearchResult>.Success(new RedblockSearchResult
        {
            Results = searchIdentifiers,
            HasMore = hasMore,
            NextCursorRedblockId = nextCursor,
            ReturnedCount = redblockList.Count,
            FailedRedblockLookups = []
        });
    }

    public async Task<Result<RedblockStatus>> AddRedblockStatus(long projectId, long keyNumber, string status, long createdBy)
    {
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var insertStatusResult = await redblockRepo.InsertStatus(projectId, keyNumber, status, createdBy);
        if (!insertStatusResult.TryGetDataNonNull(out var statusEntity))
            return Result<RedblockStatus>.Failure(insertStatusResult.ErrorMessage ?? "Failed to add status to redblock.", insertStatusResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        return Result<RedblockStatus>.Success(RedblockStatus.FromModel(statusEntity));
    }

    public async Task<Result<RedblockUserAssignment>> AddRedblockUserAssignment(long projectId, long keyNumber, long assignedTo, long createdBy)
    {
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var insertUserAssignmentResult = await redblockRepo.InsertUserAssignment(projectId, keyNumber, assignedTo, createdBy);
        if (!insertUserAssignmentResult.TryGetDataNonNull(out var userAssignmentEntity))
            return Result<RedblockUserAssignment>.Failure(insertUserAssignmentResult.ErrorMessage ?? "Failed to assign user to redblock.", insertUserAssignmentResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        return Result<RedblockUserAssignment>.Success(RedblockUserAssignment.FromModel(userAssignmentEntity));
    }

    public async Task<Result> RemoveRedblockUserAssignment(long projectId, long keyNumber, long assignedTo)
    {
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var deletionResult = await redblockRepo.DeleteUserAssignment(projectId, keyNumber, assignedTo);
        if (!deletionResult.IsSuccessful)
            return Result.Failure(deletionResult.ErrorMessage ?? "Failed to remove user assignment from redblock.", deletionResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        return Result.Success();
    }

    public async Task<Result<RedblockRoleAssignment>> AddRedblockRoleAssignment(long projectId, long keyNumber, string roleName, long createdBy)
    {
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var insertRoleAssignmentResult = await redblockRepo.InsertRoleAssignment(projectId, keyNumber, roleName, createdBy);
        if (!insertRoleAssignmentResult.TryGetDataNonNull(out var roleAssignmentEntity))
            return Result<RedblockRoleAssignment>.Failure(insertRoleAssignmentResult.ErrorMessage ?? "Failed to assign role to redblock.", insertRoleAssignmentResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        return Result<RedblockRoleAssignment>.Success(RedblockRoleAssignment.FromModel(roleAssignmentEntity));
    }

    public async Task<Result> RemoveRedblockRoleAssignment(long projectId, long keyNumber, string roleName)
    {
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var deletionResult = await redblockRepo.DeleteRoleAssignment(projectId, keyNumber, roleName);
        if (!deletionResult.IsSuccessful)
            return Result.Failure(deletionResult.ErrorMessage ?? "Failed to remove role assignment from redblock.", deletionResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        return Result.Success();
    }

    public async Task<Result<List<RedblockProject>>> GetProjects()
    {
        var redblockRepo = uow.Repository<IRedblockRepository>();

        var projectsResult = await redblockRepo.SelectProjects();
        if (!projectsResult.TryGetDataNonNull(out var projectEntities))
            return Result<List<RedblockProject>>.Failure(projectsResult.ErrorMessage ?? "Failed to retrieve redblock projects.", projectsResult.StatusCode);

        var mappedProjects = projectEntities.Select(RedblockProject.FromModel).ToList();
        return Result<List<RedblockProject>>.Success(mappedProjects);
    }

    public async Task<Result<RedblockProject>> CreateProject(string projectName, string projectKey)
    {
        if (projectKey.Length > 6)
            return Result<RedblockProject>.Failure(projectKey + " is too long.");
        
        if (projectKey.Length < 2)
            return Result<RedblockProject>.Failure(projectKey + " is too short.");
        
        if (!projectKey.All(char.IsLetter))
            return Result<RedblockProject>.Failure(projectKey + " must be only letters.");
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var insertResult = await redblockRepo.InsertProject(projectName, projectKey);
        if (!insertResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockProject>.Failure(insertResult.ErrorMessage ?? "Failed to create redblock project.", insertResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        return Result<RedblockProject>.Success(RedblockProject.FromModel(projectEntity));
    }

    public async Task<Result<RedblockProject>> GetProjectById(long projectId)
    {
        var redblockRepo = uow.Repository<IRedblockRepository>();

        var projectResult = await redblockRepo.SelectProjectById(projectId);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockProject>.Failure(projectResult.ErrorMessage ?? "Failed to retrieve redblock project.", projectResult.StatusCode);

        var mappedProject = RedblockProject.FromModel(projectEntity);
        return Result<RedblockProject>.Success(mappedProject);
    }

    public async Task<Result<RedblockProject>> GetProjectByKey(string projectKey)
    {
        var redblockRepo = uow.Repository<IRedblockRepository>();

        var projectResult = await redblockRepo.SelectProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockProject>.Failure(projectResult.ErrorMessage ?? "Failed to retrieve redblock project.", projectResult.StatusCode);

        var mappedProject = RedblockProject.FromModel(projectEntity);
        return Result<RedblockProject>.Success(mappedProject);
    }

    public async Task<Result<RedblockProject>> UpdateProject(long projectId, string projectName)
    {
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var updateResult = await redblockRepo.UpdateProject(projectId, projectName);
        if (!updateResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockProject>.Failure(updateResult.ErrorMessage ?? "Failed to update redblock project.", updateResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        return Result<RedblockProject>.Success(RedblockProject.FromModel(projectEntity));
    }
}