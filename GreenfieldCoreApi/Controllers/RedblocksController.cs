using Asp.Versioning;
using GreenfieldCoreApi.ApiModels;
using GreenfieldCoreServices.Models.Redblocks;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenfieldCoreApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class RedblocksController(IRedblockService redblockService) : ControllerBase
{

    #region Project Routes

    /// <summary>
    /// Retrieves a list of all redblock projects.
    /// </summary>
    /// <returns></returns>
    [HttpGet("projects")]
    [Authorize(Roles = "Redblocks.Read,Redblocks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(List<RedblockProject>))]
    public async Task<IActionResult> GetProjects()
    {
        var projectsResult = await redblockService.GetProjects();
        return projectsResult.IsSuccessful
            ? Ok(projectsResult.GetNonNullOrThrow())
            : Problem(statusCode: projectsResult.GetStatusCodeInt(), detail: projectsResult.ErrorMessage);
    }
    
    /// <summary>
    /// Creates a new redblock project with the specified name and key.
    /// </summary>
    /// <param name="request">The request containing the project name and key.</param>
    /// <returns>The created redblock project.</returns>
    [HttpPost("project")]
    [Authorize(Roles = "Redblocks.Write,Redblocks")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces(typeof(RedblockProject))]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
    {
        var projectResult = await redblockService.CreateProject(request.ProjectName, request.ProjectKey);
        if (!projectResult.IsSuccessful)
            return Problem(statusCode: projectResult.GetStatusCodeInt(), detail: projectResult.ErrorMessage);

        var createdProject = projectResult.GetNonNullOrThrow();
        return CreatedAtAction(nameof(GetProjectByKey),
            new { version = HttpContext.GetRequestedApiVersion()?.ToString(), projectKey = createdProject.ProjectKey },
            createdProject);
    }
    
    /// <summary>
    /// Retrieves a redblock project by its unique Key
    /// </summary>
    /// <param name="projectKey">The unique Key of the redblock project.</param>
    /// <returns>The redblock project with the specified ID.</returns>
    [HttpGet("{projectKey:regex(\\D+)}")]
    [Authorize(Roles = "Redblocks.Read,Redblocks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(RedblockProject))]
    public async Task<IActionResult> GetProjectByKey([FromRoute] string projectKey)
    {
        var projectResult = await redblockService.GetProjectByKey(projectKey);
        return projectResult.IsSuccessful
            ? Ok(projectResult.GetNonNullOrThrow())
            : Problem(statusCode: projectResult.GetStatusCodeInt(), detail: projectResult.ErrorMessage);
    }

    /// <summary>
    /// Updates the name of an existing redblock project.
    /// </summary>
    /// <param name="projectKey">The unique Key of the redblock project to update.</param>
    /// <param name="request">The request containing the new project name.</param>
    /// <returns>The updated redblock project.</returns>
    [HttpPut("{projectKey:regex(\\D+)}")]
    [Authorize(Roles = "Redblocks.Write,Redblocks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(RedblockProject))]
    public async Task<IActionResult> UpdateProject([FromRoute] string projectKey, [FromBody] UpdateProjectRequest request)
    {
        var projectResult = await redblockService.GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var project))
            return Problem(statusCode: projectResult.GetStatusCodeInt(), detail: projectResult.ErrorMessage);
        
        var updateResult = await redblockService.UpdateProject(project.ProjectId, request.ProjectName);
        return updateResult.IsSuccessful
            ? Ok(updateResult.GetNonNullOrThrow())
            : Problem(statusCode: updateResult.GetStatusCodeInt(), detail: updateResult.ErrorMessage);
    }
    
    #endregion

    #region Redblock Routes

    /// <summary>
    /// Imports redblocks in bulk using the external import payload format.
    /// </summary>
    /// <param name="request">The bulk import payload.</param>
    /// <returns>A placeholder response until business logic is implemented.</returns>
    [HttpPost("bulk")]
    [Authorize(Roles = "Redblocks.Write,Redblocks")]
    public async Task<IActionResult> BulkImportRedblocks([FromBody] BulkImportRedblocksRequest request)
    {
        var importResult = await redblockService.BulkImportRedblocks(request);
        if (!importResult.TryGetDataNonNull(out var importData))
            return Problem(statusCode: importResult.GetStatusCodeInt(), detail: importResult.ErrorMessage);

        var errors = importData.Errors.ToList();
        return errors.Count == 0
            ? Ok("Bulk import completed successfully with no errors.")
            : Ok(new { Message = "Bulk import completed with some errors.", Errors = errors });
    }

    /// <summary>
    /// Searches for redblocks in a project with optional filters, returning paginated results as lightweight identifiers.
    /// </summary>
    /// <param name="projectKey">The unique Key of the redblock project.</param>
    /// <param name="searchFilter">Search criteria including filters and pagination parameters.</param>
    /// <returns>A paginated list of redblock identifiers matching the specified criteria.</returns>
    [HttpPost("{projectKey:regex(\\D+)}/search")]
    [Authorize(Roles = "Redblocks.Read,Redblocks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(RedblockSearchResult))]
    public async Task<IActionResult> SearchRedblocks([FromRoute] string projectKey, [FromBody] RedblockSearchRequest searchFilter)
    {
        var projectKeyResult = await redblockService.GetProjectByKey(projectKey);
        if (!projectKeyResult.TryGetDataNonNull(out var project))
            return Problem(statusCode: projectKeyResult.GetStatusCodeInt(), detail: projectKeyResult.ErrorMessage);
        
        // Validate PageSize
        if (searchFilter.PageSize < 1)
            return BadRequest("PageSize must be at least 1.");
        if (searchFilter.PageSize > 500)
            return BadRequest("PageSize cannot exceed 500.");

        var redblocksResult = await redblockService.GetRedblocksByProject(project.ProjectId, searchFilter);
        return redblocksResult.IsSuccessful
            ? Ok(redblocksResult.GetNonNullOrThrow())
            : Problem(statusCode: redblocksResult.GetStatusCodeInt(), detail: redblocksResult.ErrorMessage);
    }

    /// <summary>
    /// Create a redblock
    /// </summary>
    /// <param name="projectKey"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("{projectKey:regex(\\D+)}/redblock")]
    [Authorize(Roles = "Redblocks.Write,Redblocks")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces(typeof(Redblock))]
    public async Task<IActionResult> CreateRedblock([FromRoute] string projectKey, [FromBody] CreateRedblockRequest request)
    {
        var projectKeyResult = await redblockService.GetProjectByKey(projectKey);
        if (!projectKeyResult.TryGetDataNonNull(out var project))
            return Problem(statusCode: projectKeyResult.GetStatusCodeInt(), detail: projectKeyResult.ErrorMessage);
        
        var redblockResult = await redblockService.CreateRedblock(
            project.ProjectId,
            request.X,
            request.Y,
            request.Z,
            request.Message,
            request.CreatedBy,
            request.InitialStatus,
            request.AssignedUsers,
            request.AssignedRoles);

        if (!redblockResult.IsSuccessful)
            return Problem(statusCode: redblockResult.GetStatusCodeInt(), detail: redblockResult.ErrorMessage);

        var createdRedblock = redblockResult.GetNonNullOrThrow();
        return CreatedAtAction(nameof(GetRedblockByKey),
            new
            {
                version = HttpContext.GetRequestedApiVersion()?.ToString(),
                projectKey = project.ProjectKey,
                redblockKey = createdRedblock.KeyNumber
            },
            createdRedblock);
    }
    
    /// <summary>
    /// Retrieves a redblock by its unique Key within a specific project.
    /// </summary>
    /// <param name="projectKey">The key of the project.</param>
    /// <param name="redblockKey">The unique key of the redblock.</param>
    /// <returns>The redblock if found.</returns>
    [HttpGet("{projectKey:regex(\\D+)}/{redblockKey:long}")]
    [Authorize(Roles = "Redblocks.Read,Redblocks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(Redblock))]
    public async Task<IActionResult> GetRedblockByKey([FromRoute] string projectKey, [FromRoute] long redblockKey)
    {
        var projectKeyResult = await redblockService.GetProjectByKey(projectKey);
        if (!projectKeyResult.TryGetDataNonNull(out var project))
            return Problem(statusCode: projectKeyResult.GetStatusCodeInt(), detail: projectKeyResult.ErrorMessage);
        
        var redblockResult = await redblockService.GetRedblockByKey(project.ProjectId, redblockKey);
        return redblockResult.IsSuccessful
            ? Ok(redblockResult.GetNonNullOrThrow())
            : Problem(statusCode: redblockResult.GetStatusCodeInt(), detail: redblockResult.ErrorMessage);
    }
    
    /// <summary>
    /// Updates the message of an existing redblock identified by its unique Key within a specific project.
    /// </summary>
    /// <param name="projectKey">The key of the project.</param>
    /// <param name="redblockKey">The unique key of the redblock.</param>
    /// <param name="request">The request containing the updated message.</param>
    /// <returns>The result of the update operation.</returns>
    [HttpPut("{projectKey:regex(\\D+)}/{redblockKey:long}")]
    [Authorize(Roles = "Redblocks.Write,Redblocks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRedblock([FromRoute] string projectKey, [FromRoute] long redblockKey, [FromBody] UpdateRedblockRequest request)
    {
        var projectKeyResult = await redblockService.GetProjectByKey(projectKey);
        if (!projectKeyResult.TryGetDataNonNull(out var project))
            return Problem(statusCode: projectKeyResult.GetStatusCodeInt(), detail: projectKeyResult.ErrorMessage);
        
        var updateResult = await redblockService.UpdateRedblock(project.ProjectId, redblockKey, request.Message, request.UpdatedBy);
        return updateResult.IsSuccessful
            ? Ok()
            : Problem(statusCode: updateResult.GetStatusCodeInt(), detail: updateResult.ErrorMessage);
    }

    /// <summary>
    /// Replaces all associated entity GUIDs for a specific redblock.
    /// </summary>
    /// <param name="projectKey">The key of the project.</param>
    /// <param name="keyNumber">The unique key of the redblock.</param>
    /// <param name="request">The request containing the full set of entity GUIDs.</param>
    /// <returns>The persisted list of associated entity GUIDs.</returns>
    [HttpPut("{projectKey:regex(\\D+)}/{keyNumber:long}/entities")]
    [Authorize(Roles = "Redblocks.Write,Redblocks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces(typeof(List<Guid>))]
    public async Task<IActionResult> ReplaceRedblockEntities([FromRoute] string projectKey, [FromRoute] long keyNumber, [FromBody] ReplaceRedblockEntitiesRequest request)
    {
        var projectKeyResult = await redblockService.GetProjectByKey(projectKey);
        if (!projectKeyResult.TryGetDataNonNull(out var project))
            return Problem(statusCode: projectKeyResult.GetStatusCodeInt(), detail: projectKeyResult.ErrorMessage);

        var replaceResult = await redblockService.ReplaceRedblockEntities(project.ProjectId, keyNumber, request.Entities);
        return replaceResult.IsSuccessful
            ? Ok(replaceResult.GetNonNullOrThrow())
            : Problem(statusCode: replaceResult.GetStatusCodeInt(), detail: replaceResult.ErrorMessage);
    }

    /// <summary>
    /// Removes all associated entity GUIDs for a specific redblock.
    /// </summary>
    /// <param name="projectKey">The key of the project.</param>
    /// <param name="keyNumber">The unique key of the redblock.</param>
    /// <returns>The result of the clear operation.</returns>
    [HttpDelete("{projectKey:regex(\\D+)}/{keyNumber:long}/entities")]
    [Authorize(Roles = "Redblocks.Write,Redblocks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ClearRedblockEntities([FromRoute] string projectKey, [FromRoute] long keyNumber)
    {
        var projectKeyResult = await redblockService.GetProjectByKey(projectKey);
        if (!projectKeyResult.TryGetDataNonNull(out var project))
            return Problem(statusCode: projectKeyResult.GetStatusCodeInt(), detail: projectKeyResult.ErrorMessage);

        var clearResult = await redblockService.ClearRedblockEntities(project.ProjectId, keyNumber);
        return clearResult.IsSuccessful
            ? Ok()
            : Problem(statusCode: clearResult.GetStatusCodeInt(), detail: clearResult.ErrorMessage);
    }
    
    /// <summary>
    /// Adds a new status entry to the history of a specific redblock identified by its unique Key within a specific project.
    /// </summary>
    /// <param name="projectKey">The key of the project.</param>
    /// <param name="keyNumber">The unique key of the redblock.</param>
    /// <param name="request">The request containing the status information.</param>
    /// <returns>The result of the add operation.</returns>
    [HttpPost("{projectKey:regex(\\D+)}/{keyNumber:long}/status")]
    [Authorize(Roles = "Redblocks.Write,Redblocks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces(typeof(RedblockStatus))]
    public async Task<IActionResult> AddRedblockStatus([FromRoute] string projectKey, [FromRoute] long keyNumber, [FromBody] AddStatusRequest request)
    {
        var projectKeyResult = await redblockService.GetProjectByKey(projectKey);
        if (!projectKeyResult.TryGetDataNonNull(out var project))
            return Problem(statusCode: projectKeyResult.GetStatusCodeInt(), detail: projectKeyResult.ErrorMessage);

        var statusResult = await redblockService.AddRedblockStatus(project.ProjectId, keyNumber, request.Status, request.CreatedBy);
        return statusResult.IsSuccessful
            ? Ok(statusResult.GetNonNullOrThrow())
            : Problem(statusCode: statusResult.GetStatusCodeInt(), detail: statusResult.ErrorMessage);
    }

    /// <summary>
    /// Assigns a user to a specific redblock identified by its unique Key within a specific project, granting them access to the redblock.
    /// </summary>
    /// <param name="projectKey">The key of the project.</param>
    /// <param name="keyNumber">The unique key of the redblock.</param>
    /// <param name="request">The request containing the user assignment information.</param>
    /// <returns>The result of the add operation.</returns>
    [HttpPost("{projectKey:regex(\\D+)}/{keyNumber:long}/users")]
    [Authorize(Roles = "Redblocks.Write,Redblocks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces(typeof(RedblockUserAssignment))]
    public async Task<IActionResult> AddRedblockUserAssignment([FromRoute] string projectKey, [FromRoute] long keyNumber, [FromBody] AssignUserRequest request)
    {
        var projectKeyResult = await redblockService.GetProjectByKey(projectKey);
        if (!projectKeyResult.TryGetDataNonNull(out var project))
            return Problem(statusCode: projectKeyResult.GetStatusCodeInt(), detail: projectKeyResult.ErrorMessage);

        var userAssignmentResult = await redblockService.AddRedblockUserAssignment(project.ProjectId, keyNumber, request.UserId, request.CreatedBy);
        return userAssignmentResult.IsSuccessful
            ? Ok(userAssignmentResult.GetNonNullOrThrow())
            : Problem(statusCode: userAssignmentResult.GetStatusCodeInt(), detail: userAssignmentResult.ErrorMessage);
    }

    /// <summary>
    /// Assigns a role to a specific redblock identified by its unique Key within a specific project, granting access to all users in that role.
    /// </summary>
    /// <param name="projectKey">The key of the project.</param>
    /// <param name="keyNumber">The unique key of the redblock.</param>
    /// <param name="request">The request containing the role assignment information.</param>
    /// <returns>The result of the add operation.</returns>
    [HttpPost("{projectKey:regex(\\D+)}/{keyNumber:long}/roles")]
    [Authorize(Roles = "Redblocks.Write,Redblocks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces(typeof(RedblockRoleAssignment))]
    public async Task<IActionResult> AddRedblockRoleAssignment([FromRoute] string projectKey, [FromRoute] long keyNumber, [FromBody] AssignRoleRequest request)
    {
        var projectKeyResult = await redblockService.GetProjectByKey(projectKey);
        if (!projectKeyResult.TryGetDataNonNull(out var project))
            return Problem(statusCode: projectKeyResult.GetStatusCodeInt(), detail: projectKeyResult.ErrorMessage);

        var roleAssignmentResult = await redblockService.AddRedblockRoleAssignment(project.ProjectId, keyNumber, request.RoleName, request.CreatedBy);
        return roleAssignmentResult.IsSuccessful
            ? Ok(roleAssignmentResult.GetNonNullOrThrow())
            : Problem(statusCode: roleAssignmentResult.GetStatusCodeInt(), detail: roleAssignmentResult.ErrorMessage);
    }

    /// <summary>
    /// Deletes a specific redblock identified by its unique Key within a specific project, removing it from the system along with all associated statuses and assignments.
    /// </summary>
    /// <param name="projectKey"></param>
    /// <param name="keyNumber"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpDelete("{projectKey:regex(\\D+)}/{keyNumber:long}")]
    [Authorize(Roles = "Redblocks.Write,Redblocks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteRedblock([FromRoute] string projectKey, [FromRoute] long keyNumber, [FromBody] DeleteRedblockRequest request)
    {
        var projectKeyResult = await redblockService.GetProjectByKey(projectKey);
        if (!projectKeyResult.TryGetDataNonNull(out var project))
            return Problem(statusCode: projectKeyResult.GetStatusCodeInt(), detail: projectKeyResult.ErrorMessage);
    
        var deleteResult = await redblockService.DeleteRedblock(project.ProjectId, keyNumber, request.DeletedBy);
        return deleteResult.IsSuccessful
            ? Ok()
            : Problem(statusCode: deleteResult.GetStatusCodeInt(), detail: deleteResult.ErrorMessage);
    }

    /// <summary>
    /// Removes a user assignment from a specific redblock identified by its unique Key within a specific project, revoking their access to the redblock.
    /// </summary>
    /// <param name="projectKey">The key of the project.</param>
    /// <param name="keyNumber">The unique key of the redblock.</param>
    /// <param name="userId">The ID of the user to remove from the redblock.</param>
    /// <returns>The result of the remove operation.</returns>
    [HttpDelete("{projectKey:regex(\\D+)}/{keyNumber:long}/users/{userId:long}")]
    [Authorize(Roles = "Redblocks.Write,Redblocks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveRedblockUserAssignment([FromRoute] string projectKey, [FromRoute] long keyNumber, [FromRoute] long userId)
    {
        var projectKeyResult = await redblockService.GetProjectByKey(projectKey);
        if (!projectKeyResult.TryGetDataNonNull(out var project))
            return Problem(statusCode: projectKeyResult.GetStatusCodeInt(), detail: projectKeyResult.ErrorMessage);
    
        var removeResult = await redblockService.RemoveRedblockUserAssignment(project.ProjectId, keyNumber, userId);
        return removeResult.IsSuccessful
            ? Ok()
            : Problem(statusCode: removeResult.GetStatusCodeInt(), detail: removeResult.ErrorMessage);
    }

    /// <summary>
    /// Removes a role assignment from a specific redblock identified by its unique Key within a specific project, revoking access for all users in that role to the redblock.
    /// </summary>
    /// <param name="projectKey">The key of the project.</param>
    /// <param name="keyNumber">The unique key of the redblock.</param>
    /// <param name="roleName">The name of the role to remove from the redblock.</param>
    /// <returns>The result of the remove operation.</returns>
    [HttpDelete("{projectKey:regex(\\D+)}/{keyNumber:long}/roles/{roleName}")]
    [Authorize(Roles = "Redblocks.Write,Redblocks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveRedblockRoleAssignment([FromRoute] string projectKey, [FromRoute] long keyNumber, [FromRoute] string roleName)
    {
        var projectKeyResult = await redblockService.GetProjectByKey(projectKey);
        if (!projectKeyResult.TryGetDataNonNull(out var project))
            return Problem(statusCode: projectKeyResult.GetStatusCodeInt(), detail: projectKeyResult.ErrorMessage);
    
        var removeResult = await redblockService.RemoveRedblockRoleAssignment(project.ProjectId, keyNumber, roleName);
        return removeResult.IsSuccessful
            ? Ok()
            : Problem(statusCode: removeResult.GetStatusCodeInt(), detail: removeResult.ErrorMessage);
    }
    
    #endregion
}
