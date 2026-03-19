using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Redblocks;

namespace GreenfieldCoreServices.Services.Interfaces;

public interface IRedblockService
{

    #region Redblock Service Methods

    /// <summary>
    /// Creates a new redblock at the specified coordinates within a project.
    /// </summary>
    /// <param name="projectId">The ID of the project to create the redblock in.</param>
    /// <param name="x">The X coordinate of the redblock.</param>
    /// <param name="y">The Y coordinate of the redblock.</param>
    /// <param name="z">The Z coordinate of the redblock.</param>
    /// <param name="message">The message or description associated with the redblock.</param>
    /// <param name="createdBy">The ID of the user creating the redblock.</param>
    /// <returns>A <see cref="Result{T}"/> containing the created <see cref="Redblock"/>.</returns>
    Task<Result<Redblock>> CreateRedblock(long projectId, int x, int y, int z, string message, long createdBy);

    /// <summary>
    /// Deletes a redblock identified by its key number within a project.
    /// </summary>
    /// <param name="projectId">The ID of the project the redblock belongs to.</param>
    /// <param name="keyNumber">The key number identifying the redblock within the project.</param>
    /// <returns>A <see cref="Result"/> indicating whether the deletion was successful.</returns>
    Task<Result> DeleteRedblock(long projectId, long keyNumber);

    /// <summary>
    /// Updates the message of an existing redblock.
    /// </summary>
    /// <param name="projectId">The ID of the project the redblock belongs to.</param>
    /// <param name="keyNumber">The key number identifying the redblock within the project.</param>
    /// <param name="newMessage">The new message or description to set on the redblock.</param>
    /// <param name="updatedBy">The ID of the user performing the update.</param>
    /// <returns>A <see cref="Result{T}"/> containing the updated <see cref="Redblock"/>.</returns>
    Task<Result<Redblock>> UpdateRedblock(long projectId, long keyNumber, string newMessage, long updatedBy);

    /// <summary>
    /// Retrieves a redblock by its unique ID.
    /// </summary>
    /// <param name="redblockId">The unique ID of the redblock.</param>
    /// <returns>A <see cref="Result{T}"/> containing the matching <see cref="Redblock"/>, or a failure result if not found.</returns>
    Task<Result<Redblock>> GetRedblockById(long redblockId);

    /// <summary>
    /// Retrieves a redblock by its key number within a project.
    /// </summary>
    /// <param name="projectId">The ID of the project the redblock belongs to.</param>
    /// <param name="keyNumber">The key number identifying the redblock within the project.</param>
    /// <returns>A <see cref="Result{T}"/> containing the matching <see cref="Redblock"/>, or a failure result if not found.</returns>
    Task<Result<Redblock>> GetRedblockByKey(long projectId, long keyNumber);

    /// <summary>
    /// Retrieves all redblocks within a project, optionally filtered by search criteria.
    /// </summary>
    /// <param name="projectId">The ID of the project to retrieve redblocks from.</param>
    /// <param name="searchFilter">Optional search criteria to filter the redblocks by status, deletion status, user assignments, role assignments, or message content.</param>
    /// <returns></returns>
    Task<Result<List<Redblock>>> GetRedblocksByProject(long projectId, RedblockSearchRequest? searchFilter);
    
    /// <summary>
    /// Adds a status entry to an existing redblock.
    /// </summary>
    /// <param name="projectId">The ID of the project the redblock belongs to.</param>
    /// <param name="keyNumber">The key number identifying the redblock within the project.</param>
    /// <param name="status">The status value to add to the redblock.</param>
    /// <param name="createdBy">The ID of the user adding the status.</param>
    /// <returns>A <see cref="Result{T}"/> containing the created <see cref="RedblockStatus"/>.</returns>
    Task<Result<RedblockStatus>> AddRedblockStatus(long projectId, long keyNumber, string status, long createdBy);

    /// <summary>
    /// Assigns a user to a redblock.
    /// </summary>
    /// <param name="projectId">The ID of the project the redblock belongs to.</param>
    /// <param name="keyNumber">The key number identifying the redblock within the project.</param>
    /// <param name="assignedTo">The ID of the user to assign to the redblock.</param>
    /// <param name="createdBy">The ID of the user performing the assignment.</param>
    /// <returns>A <see cref="Result{T}"/> containing the created <see cref="RedblockUserAssignment"/>.</returns>
    Task<Result<RedblockUserAssignment>> AddRedblockUserAssignment(long projectId, long keyNumber, long assignedTo, long createdBy);

    /// <summary>
    /// Removes a user assignment from a redblock.
    /// </summary>
    /// <param name="projectId">The ID of the project the redblock belongs to.</param>
    /// <param name="keyNumber">The key number identifying the redblock within the project.</param>
    /// <param name="assignedTo">The ID of the user to remove from the redblock.</param>
    /// <returns>A <see cref="Result"/> indicating whether the removal was successful.</returns>
    Task<Result> RemoveRedblockUserAssignment(long projectId, long keyNumber, long assignedTo);

    /// <summary>
    /// Assigns a role to a redblock.
    /// </summary>
    /// <param name="projectId">The ID of the project the redblock belongs to.</param>
    /// <param name="keyNumber">The key number identifying the redblock within the project.</param>
    /// <param name="roleName">The name of the role to assign to the redblock.</param>
    /// <param name="createdBy">The ID of the user performing the assignment.</param>
    /// <returns>A <see cref="Result{T}"/> containing the created <see cref="RedblockRoleAssignment"/>.</returns>
    Task<Result<RedblockRoleAssignment>> AddRedblockRoleAssignment(long projectId, long keyNumber, string roleName, long createdBy);

    /// <summary>
    /// Removes a role assignment from a redblock.
    /// </summary>
    /// <param name="projectId">The ID of the project the redblock belongs to.</param>
    /// <param name="keyNumber">The key number identifying the redblock within the project.</param>
    /// <param name="roleName">The name of the role to remove from the redblock.</param>
    /// <returns>A <see cref="Result"/> indicating whether the removal was successful.</returns>
    Task<Result> RemoveRedblockRoleAssignment(long projectId, long keyNumber, string roleName);

    #endregion

    #region Project Service Methods

    /// <summary>
    /// Retrieves all redblock projects.
    /// </summary>
    /// <returns>A <see cref="Result{T}"/> containing a list of all <see cref="RedblockProject"/> instances.</returns>
    Task<Result<List<RedblockProject>>> GetProjects();

    /// <summary>
    /// Creates a new redblock project with the specified name and key.
    /// </summary>
    /// <param name="projectName">The display name of the project.</param>
    /// <param name="projectKey">The unique short key identifier for the project (e.g. "PROJ").</param>
    /// <returns>A <see cref="Result{T}"/> containing the created <see cref="RedblockProject"/>.</returns>
    Task<Result<RedblockProject>> CreateProject(string projectName, string projectKey);

    /// <summary>
    /// Retrieves a redblock project by its unique ID.
    /// </summary>
    /// <param name="projectId">The unique ID of the project.</param>
    /// <returns>A <see cref="Result{T}"/> containing the matching <see cref="RedblockProject"/>, or a failure result if not found.</returns>
    Task<Result<RedblockProject>> GetProjectById(long projectId);

    /// <summary>
    /// Retrieves a redblock project by its unique key.
    /// </summary>
    /// <param name="projectKey">The unique short key identifier for the project.</param>
    /// <returns>A <see cref="Result{T}"/> containing the matching <see cref="RedblockProject"/>, or a failure result if not found.</returns>
    Task<Result<RedblockProject>> GetProjectByKey(string projectKey);

    /// <summary>
    /// Updates the name of an existing redblock project.
    /// </summary>
    /// <param name="projectId">The unique ID of the project to update.</param>
    /// <param name="projectName">The new display name for the project.</param>
    /// <returns>A <see cref="Result{T}"/> containing the updated <see cref="RedblockProject"/>.</returns>
    Task<Result<RedblockProject>> UpdateProject(long projectId, string projectName);

    #endregion
}