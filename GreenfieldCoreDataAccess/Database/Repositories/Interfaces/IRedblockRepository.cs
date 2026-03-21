using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreDataAccess.Database.Repositories.Interfaces;

/// <summary>
/// Repository methods for working with Redblock projects, redblocks, statuses, and assignments.
/// </summary>
public interface IRedblockRepository
{
    /// <summary>
    /// Create a new Redblock project.
    /// </summary>
    /// <param name="projectName">The display name of the project to create.</param>
    /// <param name="projectKey">The short key used to identify the project.</param>
    /// <returns>Result containing the created <see cref="RedblockProjectEntity"/>, or a failed Result if no row was returned.</returns>
    Task<Result<RedblockProjectEntity>> InsertProject(string projectName, string projectKey);

    /// <summary>
    /// Update the name of an existing Redblock project.
    /// </summary>
    /// <param name="projectId">The internal project ID of the project to update.</param>
    /// <param name="projectName">The new project name to set.</param>
    /// <returns>Result containing the updated <see cref="RedblockProjectEntity"/>, or a failed Result if the project was not found.</returns>
    Task<Result<RedblockProjectEntity>> UpdateProject(long projectId, string projectName);

    /// <summary>
    /// Get all Redblock projects.
    /// </summary>
    /// <returns>Result containing all <see cref="RedblockProjectEntity"/> entries.</returns>
    Task<Result<IEnumerable<RedblockProjectEntity>>> SelectProjects();

    /// <summary>
    /// Get a Redblock project by its internal project ID.
    /// </summary>
    /// <param name="projectId">The internal project ID of the project to retrieve.</param>
    /// <returns>Result containing the <see cref="RedblockProjectEntity"/> if found, or a failed Result if no project was found with the given ID.</returns>
    Task<Result<RedblockProjectEntity>> SelectProjectById(long projectId);

    /// <summary>
    /// Create a new Redblock in a project.
    /// </summary>
    /// <param name="projectId">The project ID that the Redblock belongs to.</param>
    /// <param name="message">The Redblock message or description.</param>
    /// <param name="x">The X coordinate of the Redblock.</param>
    /// <param name="y">The Y coordinate of the Redblock.</param>
    /// <param name="z">The Z coordinate of the Redblock.</param>
    /// <param name="createdBy">The internal user ID of the user creating the Redblock.</param>
    /// <returns>Result containing the created <see cref="RedblockEntity"/>, or a failed Result if no row was returned.</returns>
    Task<Result<RedblockEntity>> InsertRedblock(long projectId, string message, int x, int y, int z, long createdBy);

    /// <summary>
    /// Get a Redblock by its internal Redblock ID.
    /// </summary>
    /// <param name="redblockId">The internal Redblock ID to retrieve.</param>
    /// <returns>Result containing the <see cref="RedblockEntity"/> if found, or a failed Result if no Redblock was found.</returns>
    Task<Result<RedblockEntity>> SelectRedblockById(long redblockId);

    /// <summary>
    /// Get a Redblock by project ID and Redblock key number.
    /// </summary>
    /// <param name="projectId">The project ID that the Redblock belongs to.</param>
    /// <param name="keyNumber">The Redblock key number within the project.</param>
    /// <returns>Result containing the <see cref="RedblockEntity"/> if found, or a failed Result if no Redblock was found.</returns>
    Task<Result<RedblockEntity>> SelectRedblockByKey(long projectId, long keyNumber);

    /// <summary>
    /// Get all Redblocks within a project, optionally filtered by status, deletion, user assignments, role assignments, and/or message content.
    /// </summary>
    /// <param name="projectId">The project ID that the Redblocks belong to.</param>
    /// <param name="statusFilter">Optional filter for Redblock status.</param>
    /// <param name="statusFilterMatchType">Optional match type for status filter.</param>
    /// <param name="deletionFilter">Optional filter for Redblock deletion status.</param>
    /// <param name="deletionFilterMatchType">Optional match type for deletion filter.</param>
    /// <param name="userAssignmentFilter">Optional filter for user assignments.</param>
    /// <param name="userAssignmentFilterMatchType">Optional match type for user assignment filter.</param>
    /// <param name="roleAssignmentFilter">Optional filter for role assignments.</param>
    /// <param name="roleAssignmentFilterMatchType">Optional match type for role assignment filter.</param>
    /// <param name="messageFilter">Optional filter for Redblock message content.</param>
    /// <param name="messageFilterMatchType">Optional match type for message filter.</param>
    /// <returns>Result containing matching <see cref="RedblockEntity"/> rows.</returns>
    Task<Result<IEnumerable<RedblockEntity>>> SelectRedblocksByProject(long projectId, string? statusFilter, string? statusFilterMatchType, string? deletionFilter, string? deletionFilterMatchType, string? userAssignmentFilter, string? userAssignmentFilterMatchType, string? roleAssignmentFilter, string? roleAssignmentFilterMatchType, string? messageFilter, string? messageFilterMatchType);

    /// <summary>
    /// Get all status rows for a Redblock by its internal Redblock ID.
    /// </summary>
    /// <param name="redblockId">The internal Redblock ID.</param>
    /// <returns>Result containing matching <see cref="RedblockStatusEntity"/> rows.</returns>
    Task<Result<IEnumerable<RedblockStatusEntity>>> SelectRedblockStatuses(long redblockId);

    /// <summary>
    /// Get all user assignments for a Redblock by its internal Redblock ID.
    /// </summary>
    /// <param name="redblockId">The internal Redblock ID.</param>
    /// <returns>Result containing matching <see cref="RedblockUserAssignmentEntity"/> rows.</returns>
    Task<Result<IEnumerable<RedblockUserAssignmentEntity>>> SelectRedblockUserAssignments(long redblockId);

    /// <summary>
    /// Get all role assignments for a Redblock by its internal Redblock ID.
    /// </summary>
    /// <param name="redblockId">The internal Redblock ID.</param>
    /// <returns>Result containing matching <see cref="RedblockRoleAssignmentEntity"/> rows.</returns>
    Task<Result<IEnumerable<RedblockRoleAssignmentEntity>>> SelectRedblockRoleAssignments(long redblockId);

    /// <summary>
    /// Update a Redblock's message.
    /// </summary>
    /// <param name="projectId">The project ID that the Redblock belongs to.</param>
    /// <param name="keyNumber">The Redblock key number within the project.</param>
    /// <param name="message">The new message to set.</param>
    /// <param name="updatedBy"></param>
    /// <returns>Result containing the updated <see cref="RedblockEntity"/>, or a failed Result if no Redblock was found.</returns>
    Task<Result<RedblockEntity>> UpdateRedblockMessage(long projectId, long keyNumber, string message, long updatedBy);

    /// <summary>
    /// Soft delete a Redblock.
    /// </summary>
    /// <param name="projectId">The project ID that the Redblock belongs to.</param>
    /// <param name="keyNumber">The Redblock key number within the project.</param>
    /// <param name="deletedBy">The internal user ID of the user performing the delete.</param>
    /// <returns>Result containing the updated <see cref="RedblockEntity"/>, or a failed Result if no Redblock was found.</returns>
    Task<Result<RedblockEntity>> SoftDeleteRedblock(long projectId, long keyNumber, long deletedBy);

    /// <summary>
    /// Add a status entry to a Redblock.
    /// </summary>
    /// <param name="projectId">The project ID that the Redblock belongs to.</param>
    /// <param name="keyNumber">The Redblock key number within the project.</param>
    /// <param name="status">The status value to add.</param>
    /// <param name="createdBy">The internal user ID of the user creating the status entry.</param>
    /// <returns>Result containing the created <see cref="RedblockStatusEntity"/>, or a failed Result if no row was returned.</returns>
    Task<Result<RedblockStatusEntity>> InsertStatus(long projectId, long keyNumber, string status, long createdBy);

    /// <summary>
    /// Add a user assignment to a Redblock.
    /// </summary>
    /// <param name="projectId">The project ID that the Redblock belongs to.</param>
    /// <param name="keyNumber">The Redblock key number within the project.</param>
    /// <param name="assignedTo">The internal user ID being assigned to the Redblock.</param>
    /// <param name="createdBy">The internal user ID of the user creating the assignment.</param>
    /// <returns>Result containing the created <see cref="RedblockUserAssignmentEntity"/>, or a failed Result if no row was returned.</returns>
    Task<Result<RedblockUserAssignmentEntity>> InsertUserAssignment(long projectId, long keyNumber, long assignedTo, long createdBy);

    /// <summary>
    /// Remove a user assignment from a Redblock.
    /// </summary>
    /// <param name="projectId">The project ID that the Redblock belongs to.</param>
    /// <param name="keyNumber">The Redblock key number within the project.</param>
    /// <param name="assignedTo">The internal user ID whose assignment should be removed.</param>
    /// <returns>Result indicating whether a user assignment was removed.</returns>
    Task<Result> DeleteUserAssignment(long projectId, long keyNumber, long assignedTo);

    /// <summary>
    /// Add a role assignment to a Redblock.
    /// </summary>
    /// <param name="projectId">The project ID that the Redblock belongs to.</param>
    /// <param name="keyNumber">The Redblock key number within the project.</param>
    /// <param name="roleName">The role name to assign to the Redblock.</param>
    /// <param name="createdBy">The internal user ID of the user creating the assignment.</param>
    /// <returns>Result containing the created <see cref="RedblockRoleAssignmentEntity"/>, or a failed Result if no row was returned.</returns>
    Task<Result<RedblockRoleAssignmentEntity>> InsertRoleAssignment(long projectId, long keyNumber, string roleName, long createdBy);

    /// <summary>
    /// Remove a role assignment from a Redblock.
    /// </summary>
    /// <param name="projectId">The project ID that the Redblock belongs to.</param>
    /// <param name="keyNumber">The Redblock key number within the project.</param>
    /// <param name="roleName">The role name whose assignment should be removed.</param>
    /// <returns>Result indicating whether a role assignment was removed.</returns>
    Task<Result> DeleteRoleAssignment(long projectId, long keyNumber, string roleName);
}

