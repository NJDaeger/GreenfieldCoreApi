using System.Collections.Concurrent;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Extensions;
using GreenfieldCoreServices.Models.Redblocks;
using GreenfieldCoreServices.Models.Users;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Services;

public class RedblockService(IUnitOfWork uow, ILogger<IRedblockService> logger, IUserService userService, IServiceScopeFactory scopeFactory, ICacheService<string, RedblockProject> projectCache, ICacheService<string, Redblock> redblockCache) : IRedblockService
{
    #region Bulk Redblock Importing
    
    public async Task<Result<BulkImportRedblocksResult>> BulkImportRedblocks(BulkImportRedblocksRequest request)
    {
        const int maxParallelFollowupWorkers = 16;
        const int maxQueuedFollowupTasks = 128;

        var projectMap = new Dictionary<string, RedblockProject>();
        foreach (var worldProject in request.WorldProjects)
        {
            var projectResult = await GetProjectByKey(worldProject.Value.ProjectKey);
            if (!projectResult.TryGetDataNonNull(out var project))
            {
                var creationResult = await CreateProject(worldProject.Value.ProjectName, worldProject.Value.ProjectKey);
                if (!creationResult.TryGetDataNonNull(out project))
                {
                    return Result<BulkImportRedblocksResult>.Failure(
                        $"Failed to create project for world {worldProject.Key}: {creationResult.ErrorMessage}",
                        creationResult.StatusCode);
                }
            }

            projectMap[worldProject.Key] = project;
        }

        var systemUserResult = await userService.GetSystemUser();
        if (!systemUserResult.TryGetDataNonNull(out var systemUser))
            return Result<BulkImportRedblocksResult>.Failure(systemUserResult.ErrorMessage ?? "System user does not exist.", systemUserResult.StatusCode);

        var migrationIssues = new ConcurrentBag<string>();
        
        var repo = uow.Repository<IRedblockRepository>();
        var followupTaskSemaphore = new SemaphoreSlim(maxParallelFollowupWorkers);
        var pendingFollowupTasks = new List<Task>();

        foreach (var legacyRedblock in request.Redblocks)
        {
            if (!projectMap.TryGetValue(legacyRedblock.Value.Location.World, out var associatedProject))
            {
                migrationIssues.Add($"Redblock {legacyRedblock.Key} references world {legacyRedblock.Value.Location.World} which does not have an associated project in the import payload.");
                continue;
            }

            User? foundAssignedUser = null;
            if (legacyRedblock.Value.AssignedTo.HasValue)
            {
                var assignedToUser = legacyRedblock.Value.AssignedTo.Value;
                var assignedToUserResult = await userService.GetUserByUuid(assignedToUser);
                if (!assignedToUserResult.TryGetDataNonNull(out foundAssignedUser))
                {
                    migrationIssues.Add($"Redblock {legacyRedblock.Key} is assigned to user {assignedToUser} who could not be found in the system.");
                }
            }

            var createdByUser = legacyRedblock.Value.CreatedBy;
            var foundCreatedByUserResult = await userService.GetUserByUuid(createdByUser);
            if (!foundCreatedByUserResult.TryGetDataNonNull(out var foundCreatedByUser))
            {
                migrationIssues.Add($"Redblock {legacyRedblock.Key} was created by user {createdByUser} who could not be found in the system.");
                foundCreatedByUser = systemUser;
            }

            var statuses = new List<(string Status, long CreatedByUserId)> { ("Incomplete", foundCreatedByUser.UserId) };

            if (legacyRedblock.Value.CompletedBy.HasValue)
            {
                var pendingByUser = legacyRedblock.Value.CompletedBy.Value;
                var pendingByUserResult = await userService.GetUserByUuid(pendingByUser);
                if (!pendingByUserResult.TryGetDataNonNull(out var foundPendingByUser))
                {
                    migrationIssues.Add($"Redblock {legacyRedblock.Key} is pending completion by user {pendingByUser} who could not be found in the system.");
                    foundPendingByUser = systemUser;
                }

                statuses.Add(("Pending", foundPendingByUser.UserId));
            }

            if (legacyRedblock.Value.ApprovedBy.HasValue)
            {
                var approvedByUser = legacyRedblock.Value.ApprovedBy.Value;
                var approvedByUserResult = await userService.GetUserByUuid(approvedByUser);
                if (!approvedByUserResult.TryGetDataNonNull(out var foundApprovedByUser))
                {
                    migrationIssues.Add($"Redblock {legacyRedblock.Key} is approved by user {approvedByUser} who could not be found in the system.");
                    foundApprovedByUser = systemUser;
                }

                statuses.Add(("Approved", foundApprovedByUser.UserId));
            }

            var role = legacyRedblock.Value.MinRank;
            var isDeleted = legacyRedblock.Value.Status.Equals("Deleted", StringComparison.OrdinalIgnoreCase);
            var entities = legacyRedblock.Value.Armorstands ?? [];

            BulkImportFollowupWorkItem followupWorkItem;
            try
            {
                uow.BeginTransaction();

                var redblockInsertResult = await repo.InsertRedblock(
                    associatedProject.ProjectId,
                    legacyRedblock.Value.Content,
                    (int)legacyRedblock.Value.Location.X,
                    (int)legacyRedblock.Value.Location.Y,
                    (int)legacyRedblock.Value.Location.Z,
                    foundCreatedByUser.UserId);

                if (!redblockInsertResult.TryGetDataNonNull(out var redblockEntity))
                {
                    migrationIssues.Add($"Failed to create redblock for legacy redblock {legacyRedblock.Key}: {redblockInsertResult.ErrorMessage}");
                    uow.Rollback();
                    continue;
                }

                uow.CompleteAndCommit();

                followupWorkItem = new BulkImportFollowupWorkItem
                {
                    LegacyRedblockKey = legacyRedblock.Key,
                    ProjectId = associatedProject.ProjectId,
                    RedblockId = redblockEntity.RedblockId,
                    RedblockKeyNumber = redblockEntity.KeyNumber,
                    CreatedByUserId = foundCreatedByUser.UserId,
                    AssignedUserId = foundAssignedUser?.UserId,
                    AssignedUsername = foundAssignedUser?.Username,
                    Role = role,
                    Entities = entities,
                    Statuses = statuses,
                    IsDeleted = isDeleted,
                    SystemUserId = systemUser.UserId
                };
            }
            catch (Exception ex)
            {
                if (uow.HasActiveTransaction)
                    uow.Rollback();

                migrationIssues.Add($"Unexpected failure while creating base redblock for legacy redblock {legacyRedblock.Key}: {ex.Message}");
                continue;
            }
            
            pendingFollowupTasks.Add(ProcessFollowupWithThrottle(followupWorkItem, migrationIssues, followupTaskSemaphore));
            logger.LogInformation("Current followup task count {Count}", pendingFollowupTasks.Count);
            if (pendingFollowupTasks.Count < maxQueuedFollowupTasks)
                continue;

            var completedTask = await Task.WhenAny(pendingFollowupTasks);
            pendingFollowupTasks.Remove(completedTask);
            await completedTask;
        }

        await Task.WhenAll(pendingFollowupTasks);

        return Result<BulkImportRedblocksResult>.Success(new BulkImportRedblocksResult { Errors = migrationIssues.ToList() });
    }

    private async Task ProcessFollowupWithThrottle(BulkImportFollowupWorkItem workItem, ConcurrentBag<string> migrationIssues, SemaphoreSlim followupTaskSemaphore)
    {
        await followupTaskSemaphore.WaitAsync();
        try
        {
            await ProcessRedblockFollowupsAsync(workItem, migrationIssues);
        }
        finally
        {
            followupTaskSemaphore.Release();
        }
    }

    private async Task ProcessRedblockFollowupsAsync(BulkImportFollowupWorkItem workItem, ConcurrentBag<string> migrationIssues)
    {
        using var scope = scopeFactory.CreateScope();
        var scopedUow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var scopedRepo = scopedUow.Repository<IRedblockRepository>();
        
        try
        {
            logger.LogInformation("Processing follow-up work for legacy redblock {LegacyRedblockKey} with new redblock ID {RedblockId}", workItem.LegacyRedblockKey, workItem.RedblockId);
            scopedUow.BeginTransaction();

            if (workItem.Role is not null)
            {
                var roleInsertResult = await scopedRepo.InsertRoleAssignment(workItem.ProjectId, workItem.RedblockKeyNumber, workItem.Role, workItem.CreatedByUserId);
                if (!roleInsertResult.IsSuccessful)
                    migrationIssues.Add($"Failed to assign role '{workItem.Role}' to redblock {workItem.RedblockId} for legacy redblock {workItem.LegacyRedblockKey}: {roleInsertResult.ErrorMessage}");
            }

            if (workItem.AssignedUserId.HasValue)
            {
                var userAssignmentInsertResult = await scopedRepo.InsertUserAssignment(workItem.ProjectId, workItem.RedblockKeyNumber, workItem.AssignedUserId.Value, workItem.CreatedByUserId);
                if (!userAssignmentInsertResult.IsSuccessful)
                    migrationIssues.Add($"Failed to assign user '{workItem.AssignedUsername ?? workItem.AssignedUserId.Value.ToString()}' to redblock {workItem.RedblockId} for legacy redblock {workItem.LegacyRedblockKey}: {userAssignmentInsertResult.ErrorMessage}");
            }

            foreach (var entity in workItem.Entities)
            {
                var entityInsertResult = await scopedRepo.InsertRedblockEntity(workItem.ProjectId, workItem.RedblockKeyNumber, entity);
                if (!entityInsertResult.IsSuccessful)
                    migrationIssues.Add($"Failed to add entity {entity} to redblock {workItem.RedblockId} for legacy redblock {workItem.LegacyRedblockKey}: {entityInsertResult.ErrorMessage}");
            }

            foreach (var status in workItem.Statuses)
            {
                await Task.Delay(1000);
                var statusInsertResult = await scopedRepo.InsertStatus(workItem.ProjectId, workItem.RedblockKeyNumber, status.Status, status.CreatedByUserId);
                if (!statusInsertResult.IsSuccessful)
                    migrationIssues.Add($"Failed to add status '{status.Status}' to redblock {workItem.RedblockId} for legacy redblock {workItem.LegacyRedblockKey}: {statusInsertResult.ErrorMessage}");
            }

            if (workItem.IsDeleted)
            {
                var deletionResult = await scopedRepo.SoftDeleteRedblock(workItem.ProjectId, workItem.RedblockKeyNumber, workItem.SystemUserId);
                if (!deletionResult.IsSuccessful)
                    migrationIssues.Add($"Failed to delete redblock {workItem.RedblockId} for legacy redblock {workItem.LegacyRedblockKey}: {deletionResult.ErrorMessage}");
            }

            scopedUow.CompleteAndCommit();
            logger.LogInformation("Completed follow-up work for legacy redblock {LegacyRedblockKey} with new redblock ID {RedblockId}", workItem.LegacyRedblockKey, workItem.RedblockId);
        }
        catch (Exception ex)
        {
            if (scopedUow.HasActiveTransaction)
                scopedUow.Rollback();

            migrationIssues.Add($"Unexpected failure while running follow-up inserts for legacy redblock {workItem.LegacyRedblockKey}: {ex.Message}");
        }
    }

    #endregion

    #region Redblock Operations
    
    public async Task<Result<Redblock>> CreateRedblock(string projectKey, int x, int y, int z, string message, long createdBy, string initialStatus, List<long> assignedUsers, List<string> assignedRoles)
    {
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<Redblock>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        if (string.IsNullOrWhiteSpace(initialStatus))
            return Result<Redblock>.Failure("Initial status cannot be empty.");
        
        if (string.IsNullOrWhiteSpace(message))
            return Result<Redblock>.Failure("Message cannot be empty.");
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();

        var insertResult = await redblockRepo.InsertRedblock(projectEntity.ProjectId, message, x, y, z, createdBy);
        
        if (!insertResult.TryGetDataNonNull(out var redblockEntity))
            return Result<Redblock>.Failure(insertResult.ErrorMessage ?? "Failed to create redblock.", insertResult.StatusCode);
        
        var insertStatus = await redblockRepo.InsertStatus(projectEntity.ProjectId, redblockEntity.KeyNumber, initialStatus, createdBy);
        if (!insertStatus.TryGetDataNonNull(out var statusEntity))
            return Result<Redblock>.Failure(insertStatus.ErrorMessage ?? "Failed to set initial status on redblock.", insertStatus.StatusCode);

        var actualAssignedUsers = new List<RedblockUserAssignment>();
        var actualAssignedRoles = new List<RedblockRoleAssignment>();
        
        foreach (var userId in assignedUsers)
        {
            var insertUserResult = await redblockRepo.InsertUserAssignment(projectEntity.ProjectId, redblockEntity.KeyNumber, userId, createdBy);
            if (!insertUserResult.TryGetDataNonNull(out var assignedUser))
                logger.LogWarning("Failed to assign user {UserId} to redblock {RedblockId} in project {ProjectId}: {ErrorMessage}", userId, redblockEntity.RedblockId, projectEntity.ProjectId, insertUserResult.ErrorMessage);
            else
                actualAssignedUsers.Add(RedblockUserAssignment.FromModel(assignedUser));
        }

        foreach (var roleName in assignedRoles)
        {
            var insertRoleResult = await redblockRepo.InsertRoleAssignment(projectEntity.ProjectId, redblockEntity.KeyNumber, roleName, createdBy);
            if (!insertRoleResult.TryGetDataNonNull(out var assignedRole))
                logger.LogWarning("Failed to assign role {RoleName} to redblock {RedblockId} in project {ProjectId}: {ErrorMessage}", roleName, redblockEntity.RedblockId, projectEntity.ProjectId, insertRoleResult.ErrorMessage);
            else
                actualAssignedRoles.Add(RedblockRoleAssignment.FromModel(assignedRole));
        }

        uow.CompleteAndCommit();
        
        var mappedRedblock = Redblock.FromModel((redblockEntity, projectEntity, [RedblockStatus.FromModel(statusEntity)], actualAssignedUsers, actualAssignedRoles, []));
        
        redblockCache.SetValue(mappedRedblock.Key, mappedRedblock);
        
        return Result<Redblock>.Success(mappedRedblock);
    }

    public async Task<Result> DeleteRedblock(string projectKey, long keyNumber, long deletedBy)
    {
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<Redblock>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var deletionResult = await redblockRepo.SoftDeleteRedblock(projectEntity.ProjectId, keyNumber, deletedBy);
        if (!deletionResult.IsSuccessful)
            return Result.Failure(deletionResult.ErrorMessage ?? "Failed to delete redblock.", deletionResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        redblockCache.RemoveValue(projectEntity.ProjectKey + "-" + keyNumber);
        
        return Result.Success();
    }

    public async Task<Result> UpdateRedblock(string projectKey, long keyNumber, string newMessage, long updatedBy)
    {
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<Redblock>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var updateResult = await redblockRepo.UpdateRedblockMessage(projectEntity.ProjectId, keyNumber, newMessage, updatedBy);
        if (!updateResult.TryGetDataNonNull(out var redblockEntity))
            return Result.Failure(updateResult.ErrorMessage ?? "Failed to update redblock message.", updateResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        redblockCache.RemoveValue(projectEntity.ProjectKey + "-" + keyNumber);
        _ = await GetRedblockByKey(projectKey, keyNumber);
        
        return Result.Success();
    }

    public async Task<Result<Redblock>> GetRedblockByKey(string projectKey, long keyNumber)
    {
        if (redblockCache.TryGetValue(projectKey + "-" + keyNumber, out var cachedRedblock))
            return Result<Redblock>.Success(cachedRedblock);
        
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<Redblock>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        
        var redblockResult = await redblockRepo.SelectRedblockByKey(projectEntity.ProjectId, keyNumber);
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

        var entitiesResult = await redblockRepo.SelectRedblockEntities(redblockEntity.RedblockId);
        if (!entitiesResult.TryGetDataNonNull(out var foundEntities))
            return Result<Redblock>.Failure(entitiesResult.ErrorMessage ?? "Failed to retrieve redblock entities.", entitiesResult.StatusCode);

        var mappedRedblock = Redblock.FromModel((redblockEntity, 
            projectEntity, 
            statusEntities.Select(RedblockStatus.FromModel).ToList(),
            userAssignmentEntities.Select(RedblockUserAssignment.FromModel).ToList(),
            roleAssignmentEntities.Select(RedblockRoleAssignment.FromModel).ToList(),
            foundEntities.ToList()));
        
        redblockCache.SetValue(mappedRedblock.Key, mappedRedblock);
        
        return Result<Redblock>.Success(mappedRedblock);
    }

    public async Task<Result<RedblockSearchResult>> SearchRedblocks(string projectKey, RedblockSearchRequest searchFilter)
    {
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockSearchResult>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        if (searchFilter.CurrentPage < 1)
            return Result<RedblockSearchResult>.Failure("Current page must be greater or equal to 1.");
        
        if (searchFilter.ResultsPerPage < 1)
            return Result<RedblockSearchResult>.Failure("Results per page must be greater than or equal to 1");
        
        if (searchFilter.Location is not null && searchFilter.Radius is not null && searchFilter.Radius < 0)
            return Result<RedblockSearchResult>.Failure("Radius filter cannot be negative.");
        
        if (searchFilter.StatusFilter is not null && !searchFilter.StatusFilter.MatchType.IsAnyOf("or", "not"))
            return Result<RedblockSearchResult>.Failure("Status filter must have a match type of either 'or' or 'not'.");
        
        if (searchFilter.DeletionFilter is not null && !searchFilter.DeletionFilter.MatchType.IsAnyOf("or", "not", "and"))
            return Result<RedblockSearchResult>.Failure("Deletion filter must have a match type of either 'or' or 'not' or 'and'.");
        
        if (searchFilter.UserAssignmentFilter is not null && !searchFilter.UserAssignmentFilter.MatchType.IsAnyOf("or", "not", "and"))
            return Result<RedblockSearchResult>.Failure("User assignment filter must have a match type of either 'or' or 'not' or 'and'.");
        
        if (searchFilter.RoleAssignmentFilter is not null && !searchFilter.RoleAssignmentFilter.MatchType.IsAnyOf("or", "not", "and"))
            return Result<RedblockSearchResult>.Failure("Role assignment filter must have a match type of either 'or' or 'not' or 'and'.");
        
        if (searchFilter.MessageFilter is not null && !searchFilter.MessageFilter.MatchType.IsAnyOf("contains", "startsWith", "endsWith", "exact"))
            return Result<RedblockSearchResult>.Failure("Message filter must have a match type of either 'contains', 'startsWith', 'endsWith', or 'exact'.");
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        
        var searchResult = await redblockRepo.SelectRedblocksByProject(projectEntity.ProjectId, 
            searchFilter.Location, searchFilter.Radius,
            searchFilter.StatusFilter?.Statuses ?? [], searchFilter.StatusFilter?.MatchType,
            searchFilter.DeletionFilter?.Users ?? [], searchFilter.DeletionFilter?.MatchType,
            searchFilter.UserAssignmentFilter?.Users ?? [], searchFilter.UserAssignmentFilter?.MatchType,
            searchFilter.RoleAssignmentFilter?.Roles ?? [], searchFilter.RoleAssignmentFilter?.MatchType,
            searchFilter.MessageFilter?.Content ?? "", searchFilter.MessageFilter?.MatchType, 
            searchFilter.ResultsPerPage, searchFilter.CurrentPage);

        if (!searchResult.TryGetDataNonNull(out var actualSearchResult))
            return Result<RedblockSearchResult>.Failure(searchResult.ErrorMessage ?? "Failed to search redblocks.", searchResult.StatusCode);
        
        var mappedResults = actualSearchResult.Redblocks.Select(e => SearchedRedblockResult.FromModel((e, projectKey))).ToList();
        return Result<RedblockSearchResult>.Success(new RedblockSearchResult
        {
            Results = mappedResults,
            TotalResults = actualSearchResult.TotalResults
        });
    }

    public async Task<Result<RedblockStatus>> AddRedblockStatus(string projectKey, long keyNumber, string status, long createdBy)
    {
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockStatus>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var insertStatusResult = await redblockRepo.InsertStatus(projectEntity.ProjectId, keyNumber, status, createdBy);
        if (!insertStatusResult.TryGetDataNonNull(out var statusEntity))
            return Result<RedblockStatus>.Failure(insertStatusResult.ErrorMessage ?? "Failed to add status to redblock.", insertStatusResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        if (redblockCache.TryGetValue(projectEntity.ProjectKey + "-" + keyNumber, out var cachedRedblock))
        {
            cachedRedblock.Statuses.Add(RedblockStatus.FromModel(statusEntity));
            redblockCache.SetValue(projectEntity.ProjectKey + "-" + keyNumber, cachedRedblock);
        }
        
        return Result<RedblockStatus>.Success(RedblockStatus.FromModel(statusEntity));
    }

    public async Task<Result<RedblockUserAssignment>> AddRedblockUserAssignment(string projectKey, long keyNumber, long assignedTo, long createdBy)
    {
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockUserAssignment>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var insertUserAssignmentResult = await redblockRepo.InsertUserAssignment(projectEntity.ProjectId, keyNumber, assignedTo, createdBy);
        if (!insertUserAssignmentResult.TryGetDataNonNull(out var userAssignmentEntity))
            return Result<RedblockUserAssignment>.Failure(insertUserAssignmentResult.ErrorMessage ?? "Failed to assign user to redblock.", insertUserAssignmentResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        if (redblockCache.TryGetValue(projectEntity.ProjectKey + "-" + keyNumber, out var cachedRedblock))
        {
            cachedRedblock.UserAssignments.Add(RedblockUserAssignment.FromModel(userAssignmentEntity));
            redblockCache.SetValue(projectEntity.ProjectKey + "-" + keyNumber, cachedRedblock);
        }
        
        return Result<RedblockUserAssignment>.Success(RedblockUserAssignment.FromModel(userAssignmentEntity));
    }

    public async Task<Result> RemoveRedblockUserAssignment(string projectKey, long keyNumber, long assignedTo)
    {
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockStatus>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var deletionResult = await redblockRepo.DeleteUserAssignment(projectEntity.ProjectId, keyNumber, assignedTo);
        if (!deletionResult.IsSuccessful)
            return Result.Failure(deletionResult.ErrorMessage ?? "Failed to remove user assignment from redblock.", deletionResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        if (redblockCache.TryGetValue(projectEntity.ProjectKey + "-" + keyNumber, out var cachedRedblock))
        {
            cachedRedblock.UserAssignments.RemoveAll(ua => ua.AssignedTo == assignedTo);
            redblockCache.SetValue(projectEntity.ProjectKey + "-" + keyNumber, cachedRedblock);
        }
        
        return Result.Success();
    }

    public async Task<Result<RedblockRoleAssignment>> AddRedblockRoleAssignment(string projectKey, long keyNumber, string roleName, long createdBy)
    {
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockRoleAssignment>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var insertRoleAssignmentResult = await redblockRepo.InsertRoleAssignment(projectEntity.ProjectId, keyNumber, roleName, createdBy);
        if (!insertRoleAssignmentResult.TryGetDataNonNull(out var roleAssignmentEntity))
            return Result<RedblockRoleAssignment>.Failure(insertRoleAssignmentResult.ErrorMessage ?? "Failed to assign role to redblock.", insertRoleAssignmentResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        if (redblockCache.TryGetValue(projectEntity.ProjectKey + "-" + keyNumber, out var cachedRedblock))
        {
            cachedRedblock.RoleAssignments.Add(RedblockRoleAssignment.FromModel(roleAssignmentEntity));
            redblockCache.SetValue(projectEntity.ProjectKey + "-" + keyNumber, cachedRedblock);
        }
        
        return Result<RedblockRoleAssignment>.Success(RedblockRoleAssignment.FromModel(roleAssignmentEntity));
    }

    public async Task<Result> RemoveRedblockRoleAssignment(string projectKey, long keyNumber, string roleName)
    {
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockStatus>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var deletionResult = await redblockRepo.DeleteRoleAssignment(projectEntity.ProjectId, keyNumber, roleName);
        if (!deletionResult.IsSuccessful)
            return Result.Failure(deletionResult.ErrorMessage ?? "Failed to remove role assignment from redblock.", deletionResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        if (redblockCache.TryGetValue(projectEntity.ProjectKey + "-" + keyNumber, out var cachedRedblock))
        {
            cachedRedblock.RoleAssignments.RemoveAll(ra => ra.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));
            redblockCache.SetValue(projectEntity.ProjectKey + "-" + keyNumber, cachedRedblock);
        }
        
        return Result.Success();
    }

    public async Task<Result<List<Guid>>> ReplaceRedblockEntities(string projectKey, long keyNumber, List<Guid> entities)
    {
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<List<Guid>>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        var distinctEntities = entities.Distinct().ToList();

        uow.BeginTransaction();

        var deleteResult = await redblockRepo.DeleteRedblockEntities(projectEntity.ProjectId, keyNumber);
        if (!deleteResult.IsSuccessful)
            return Result<List<Guid>>.Failure(deleteResult.ErrorMessage ?? "Failed to clear redblock entities.", deleteResult.StatusCode);

        foreach (var entityGuid in distinctEntities)
        {
            var insertResult = await redblockRepo.InsertRedblockEntity(projectEntity.ProjectId, keyNumber, entityGuid);
            if (!insertResult.IsSuccessful)
                return Result<List<Guid>>.Failure(insertResult.ErrorMessage ?? "Failed to replace redblock entities.", insertResult.StatusCode);
        }

        uow.CompleteAndCommit();
        
        if (redblockCache.TryGetValue(projectEntity.ProjectKey + "-" + keyNumber, out var cachedRedblock))
        {
            cachedRedblock.Entities = distinctEntities;
            redblockCache.SetValue(projectEntity.ProjectKey + "-" + keyNumber, cachedRedblock);
        }
        
        return Result<List<Guid>>.Success(distinctEntities);
    }

    public async Task<Result> ClearRedblockEntities(string projectKey, long keyNumber)
    {
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockStatus>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();

        var redblockResult = await redblockRepo.SelectRedblockByKey(projectEntity.ProjectId, keyNumber);
        if (!redblockResult.IsSuccessful)
            return Result.Failure(redblockResult.ErrorMessage ?? "Redblock not found.", redblockResult.StatusCode);

        var deleteResult = await redblockRepo.DeleteRedblockEntities(projectEntity.ProjectId, keyNumber);
        if (!deleteResult.IsSuccessful)
            return Result.Failure(deleteResult.ErrorMessage ?? "Failed to clear redblock entities.", deleteResult.StatusCode);

        uow.CompleteAndCommit();
        
        if (redblockCache.TryGetValue(projectEntity.ProjectKey + "-" + keyNumber, out var cachedRedblock))
        {
            cachedRedblock.Entities = [];
            redblockCache.SetValue(projectEntity.ProjectKey + "-" + keyNumber, cachedRedblock);
        }
        
        return Result.Success();
    }

    #endregion
    
    #region Project Operations
    
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
        
        projectCache.SetValue(projectEntity.ProjectKey, RedblockProject.FromModel(projectEntity));
        
        return Result<RedblockProject>.Success(RedblockProject.FromModel(projectEntity));
    }

    public async Task<Result<RedblockProject>> GetProjectById(long projectId)
    {
        if (projectCache.TryGetValue(p => p.ProjectId == projectId, out var cachedProject))
            return Result<RedblockProject>.Success(cachedProject);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();

        var projectResult = await redblockRepo.SelectProjectById(projectId);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockProject>.Failure(projectResult.ErrorMessage ?? "Failed to retrieve redblock project.", projectResult.StatusCode);

        var mappedProject = RedblockProject.FromModel(projectEntity);
        
        projectCache.SetValue(projectEntity.ProjectKey, mappedProject);
        
        return Result<RedblockProject>.Success(mappedProject);
    }

    public async Task<Result<RedblockProject>> GetProjectByKey(string projectKey)
    {
        if (projectCache.TryGetValue(projectKey, out var cachedProject))
            return Result<RedblockProject>.Success(cachedProject);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();

        var projectResult = await redblockRepo.SelectProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockProject>.Failure(projectResult.ErrorMessage ?? "Failed to retrieve redblock project.", projectResult.StatusCode);

        var mappedProject = RedblockProject.FromModel(projectEntity);
        
        projectCache.SetValue(projectEntity.ProjectKey, mappedProject);
        
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
        
        projectCache.SetValue(projectEntity.ProjectKey, RedblockProject.FromModel(projectEntity));
        
        return Result<RedblockProject>.Success(RedblockProject.FromModel(projectEntity));
    }
    
    #endregion
}