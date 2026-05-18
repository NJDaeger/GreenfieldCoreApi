using Asp.Versioning;
using GreenfieldCoreApi.ApiModels;
using GreenfieldCoreApi.ApiModels.Connections;
using GreenfieldCoreServices.Models.BuildApps;
using GreenfieldCoreServices.Models.Connections.Discord;
using GreenfieldCoreServices.Models.Connections.Patreon;
using GreenfieldCoreServices.Models.Users;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenfieldCoreApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class UserController(IUserService userService, IPatreonService patreonService, IDiscordService discordService, IBuilderApplicationService applicationService) : ControllerBase
{
    /// <summary>
    /// Gets all users. By default, returns cached users if the cache is populated. Set <paramref name="skipCache"/> to true to force a fresh database query and repopulate the cache.
    /// </summary>
    /// <param name="skipCache">If true, bypasses the cache, queries the database directly, and repopulates the cache with the results.</param>
    /// <returns>All users.</returns>
    [HttpGet("all")]
    [Authorize(Roles = "Users.Bulk.Read,Users.Bulk,Users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(IEnumerable<GreenfieldCoreServices.Models.Users.User>))]
    public async Task<IActionResult> GetAllUsers([FromQuery] bool skipCache = false)
    {
        var result = await userService.GetAllUsers(skipCache);
        return result.IsSuccessful
            ? Ok(result.GetNonNullOrThrow())
            : Problem(statusCode: result.GetStatusCodeInt(), detail: result.ErrorMessage);
    }

    /// <summary>
    /// Gets a user by their Minecraft UUID.
    /// </summary>
    /// <param name="minecraftUuid">The Minecraft UUID of the user.</param>
    /// <returns>The user with the specified Minecraft UUID.</returns>
    [HttpGet("{minecraftUuid:guid}")]
    [Authorize(Roles = "Users.Read,Users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(GreenfieldCoreServices.Models.Users.User))]
    public async Task<IActionResult> GetUserByUuid([FromRoute] Guid minecraftUuid)
    {
        var userResult = await userService.GetUserByUuid(minecraftUuid);
        return userResult.IsSuccessful
            ? Ok(userResult.GetNonNullOrThrow())
            : Problem(statusCode: userResult.GetStatusCodeInt(), detail: userResult.ErrorMessage);
    }
    
    /// <summary>
    /// Gets a user by their internal user ID.
    /// </summary>
    /// <param name="userId">The internal user ID of the user.</param>
    /// <returns>The user with the specified internal user ID.</returns>
    [HttpGet("{userId:long}")]
    [Authorize(Roles = "Users.Read,Users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(GreenfieldCoreServices.Models.Users.User))]
    public async Task<IActionResult> GetUserByUserId([FromRoute] long userId)
    {
        var userResult = await userService.GetUserByUserId(userId);
        return userResult.IsSuccessful
            ? Ok(userResult.GetNonNullOrThrow())
            : Problem(statusCode: userResult.GetStatusCodeInt(), detail: userResult.ErrorMessage);
    }
    
    /// <summary>
    /// Updates a user's Minecraft username.
    /// </summary>
    /// <param name="minecraftUuid">The Minecraft UUID of the user.</param>
    /// <param name="username">The new username for the user.</param>
    /// <returns>The updated user.</returns>
    [HttpPatch("{minecraftUuid:guid}")]
    [Authorize(Roles = "Users.Write,Users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(GreenfieldCoreServices.Models.Users.User))]
    public async Task<IActionResult> UpdateUsername([FromRoute] Guid minecraftUuid, [FromBody] UsernameModel username) 
    {
        var updateUserResult = await userService.UpdateUsername(minecraftUuid, username.Username);
        if (!updateUserResult.TryGetDataNonNull(out var updatedUsers))
            return Problem(statusCode: updateUserResult.GetStatusCodeInt(), detail: updateUserResult.ErrorMessage);

        var updatedRequestedUser = updatedUsers.FirstOrDefault(user => user.MinecraftUuid == minecraftUuid);
        return updatedRequestedUser is null
            ? Ok(Array.Empty<User>())
            : Ok(updatedRequestedUser);
    }

    /// <summary>
    /// Refreshes usernames for the provided user IDs using Mojang profile data. Returns only users whose usernames were updated in the database.
    /// </summary>
    /// <param name="request">The request containing the user IDs to refresh.</param>
    /// <returns>All users updated as part of the refresh operation. May include users not included in the initial request if there was a username collision.</returns>
    [HttpPost("refresh")]
    [Authorize(Roles = "Users.Write,Users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(IEnumerable<GreenfieldCoreServices.Models.Users.User>))]
    public async Task<IActionResult> RefreshUsernames([FromBody] RefreshUsersRequest request)
    {
        var refreshResult = await userService.RefreshUsernames(request.UserIds);
        return !refreshResult.TryGetDataNonNull(out var updatedUsers) 
            ? Problem(statusCode: refreshResult.GetStatusCodeInt(), detail: refreshResult.ErrorMessage) 
            : Ok(updatedUsers);
    }

    /// <summary>
    /// Creates a new user with the specified Minecraft UUID and username.
    /// </summary>
    /// <param name="minecraftGuid">The Minecraft UUID of the user.</param>
    /// <param name="username">The username for the new user.</param>
    /// <returns>The created user.</returns>
    [HttpPut("{minecraftGuid:guid}")]
    [Authorize(Roles = "Users.Write,Users")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Produces(typeof(GreenfieldCoreServices.Models.Users.User))]
    public async Task<IActionResult> CreateUser([FromRoute] Guid minecraftGuid, [FromBody] UsernameModel username)
    {
        var createdUserResult = await userService.CreateUser(minecraftGuid, username.Username);
        if (!createdUserResult.IsSuccessful)
            return Problem(statusCode: createdUserResult.GetStatusCodeInt(), detail: createdUserResult.ErrorMessage);
        var created = createdUserResult.GetNonNullOrThrow();
        return CreatedAtAction(nameof(GetUserByUuid), new { version = HttpContext.GetRequestedApiVersion()?.ToString(), minecraftUuid = created.MinecraftUuid }, created);
    }
    
    /// <summary>
    /// Bulk imports users in a single service call. Each user with a Minecraft UUID that does not already exist will be created, while entries with Minecraft UUIDs that already exist will be skipped. The result contains lists of created and skipped Minecraft UUIDs.
    /// </summary>
    /// <param name="request">The request containing the users to be imported.</param>
    /// <returns>The result of the bulk import operation.</returns>
    [HttpPost("bulk")]
    [Authorize(Roles = "Users.Bulk.Write,Users.Bulk,Users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(BulkImportUsersResult))]
    public async Task<IActionResult> BulkImportUsers([FromBody] BulkImportUsersRequest request)
    {
        var importResult = await userService.BulkImportUsers(request.Users.Select(entry => new BulkImportUserEntry
        {
            Uuid = entry.Uuid,
            Username = entry.Username
        }));
        return !importResult.TryGetDataNonNull(out var data) 
            ? Problem(statusCode: importResult.GetStatusCodeInt(), detail: importResult.ErrorMessage) 
            : Ok(data);
    }

    /// <summary>
    /// Gets the latest status of all builder applications submitted by a user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>The latest status of the user's builder applications.</returns>
    [HttpGet("{userId:long}/applications")]
    [Authorize(Roles = "Users.Read.Applications,Users.Applications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(IEnumerable<ApplicationLatestStatus>))]
    public async Task<IActionResult> GetApplicationsFromUser(long userId)
    {
        var appsResult = await applicationService.GetApplicationsFromUser(userId);
        return appsResult.IsSuccessful
            ? Ok(appsResult.GetNonNullOrThrow())
            : Problem(statusCode: appsResult.GetStatusCodeInt(), detail: appsResult.ErrorMessage);
    }

    /// <summary>
    /// Unlinks a Discord account from a user's account, removing the association between the two accounts. This does not delete the Discord connection itself, but simply removes the link between the user's account and the specified Discord connection.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="discordConnectionId">The ID of the Discord connection to be unlinked.</param>
    /// <returns>The result of the unlink operation.</returns>
    [HttpDelete("{userId:long}/accounts/discord/{discordConnectionId:long}")]
    [Authorize(Roles = "Users.Write.Discord,Users.Discord,Users.Connections")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkDiscordAccount([FromRoute] long userId, [FromRoute] long discordConnectionId)
    {
        var unlinkResult = await discordService.UnlinkUserDiscordConnection(userId, discordConnectionId);
        return unlinkResult.IsSuccessful
            ? Ok()
            : Problem(statusCode: unlinkResult.GetStatusCodeInt(), detail: unlinkResult.ErrorMessage);
    }
    
    /// <summary>
    /// Gets all Discord accounts linked to a user's account, returning details about each linked Discord account including the Discord username and snowflake. This endpoint retrieves the user's Discord connections and the associated Discord account information for each connection, returning a list of linked Discord accounts for the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A list of linked Discord accounts for the specified user.</returns>
    [HttpGet("{userId:long}/accounts/discord")]
    [Authorize(Roles = "Users.Read.Discord,Users.Discord,Users.Connections")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(IEnumerable<ApiDiscordAccount>))]
    public async Task<IActionResult> GetDiscordAccountsByUserId([FromRoute] long userId)
    {
        var userResult = await userService.GetUserByUserId(userId);
        if (!userResult.TryGetDataNonNull(out var user))
            return Problem(statusCode: userResult.GetStatusCodeInt(), detail: userResult.ErrorMessage);
        
        var userDiscordConnectionsResult = await discordService.GetUserDiscordConnections(userId);
        if (!userDiscordConnectionsResult.TryGetDataNonNull(out var userDiscordConnectionsEnum))
            return Problem(statusCode: userDiscordConnectionsResult.GetStatusCodeInt(), detail: userDiscordConnectionsResult.ErrorMessage);
        
        var userDiscordConnections = userDiscordConnectionsEnum.ToList();
        var connections = new Dictionary<long, DiscordConnection>();
        foreach (var userDiscordConnection in userDiscordConnections)
        {
            var connectionResult = await discordService.GetDiscordConnection(userDiscordConnection.DiscordConnectionId);
            if (!connectionResult.TryGetDataNonNull(out var connection))
                return Problem(statusCode: connectionResult.GetStatusCodeInt(), detail: connectionResult.ErrorMessage);
            
            connections[userDiscordConnection.DiscordConnectionId] = connection;
        }
        
        var apiModels = userDiscordConnections.Select(model => new ApiDiscordAccount {
            UserDiscordConnectionId = model.UserDiscordConnectionId,
            User = user,
            DiscordConnectionId = model.DiscordConnectionId,
            DiscordSnowflake = connections[model.DiscordConnectionId].DiscordSnowflake,
            DiscordUsername = connections[model.DiscordConnectionId].DiscordUsername,
            ConnectedOn = model.ConnectedOn,
            UpdatedOn = connections[model.DiscordConnectionId].UpdatedOn,
            CreatedOn = connections[model.DiscordConnectionId].CreatedOn
        });
        
        return Ok(apiModels);
    }

    /// <summary>
    /// Unlinks a Patreon account from a user's account, removing the association between the two accounts. This does not delete the Patreon connection itself, but simply removes the link between the user's account and the specified Patreon connection.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="patreonConnectionId">The ID of the Patreon connection to be unlinked.</param>
    /// <returns>The result of the unlink operation.</returns>
    [HttpDelete("{userId:long}/accounts/patreon/{patreonConnectionId:long}")]
    [Authorize(Roles = "Users.Write.Patreon,Users.Patreon,Users.Connections")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkPatreonAccount([FromRoute] long userId, [FromRoute] long patreonConnectionId)
    {

        var unlinkResult = await patreonService.UnlinkUserPatreonConnection(userId, patreonConnectionId);
        return unlinkResult.IsSuccessful
            ? Ok()
            : Problem(statusCode: unlinkResult.GetStatusCodeInt(), detail: unlinkResult.ErrorMessage);
    }

    /// <summary>
    /// Gets all Patreon accounts linked to a user's account, returning details about each linked Patreon account including the Patreon full name and pledge amount. This endpoint retrieves the user's Patreon connections and the associated Patreon account information for each connection, returning a list of linked Patreon accounts for the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A list of linked Patreon accounts for the specified user.</returns>
    [HttpGet("{userId:long}/accounts/patreon")]
    [Authorize(Roles = "Users.Read.Patreon,Users.Patreon,Users.Connections")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(IEnumerable<ApiPatreonAccount>))]
    public async Task<IActionResult> GetPatreonAccountsByUserId([FromRoute] long userId)
    {
        var userResult = await userService.GetUserByUserId(userId);
        if (!userResult.TryGetDataNonNull(out var user))
            return Problem(statusCode: userResult.GetStatusCodeInt(), detail: userResult.ErrorMessage);
        
        var userConnectionResult = await patreonService.GetUserPatreonConnections(userId);
        if (!userConnectionResult.TryGetDataNonNull(out var userPatreonConnectionsEnum))
            return Problem(statusCode: userConnectionResult.GetStatusCodeInt(), detail: userConnectionResult.ErrorMessage);

        var userPatreonConnections = userPatreonConnectionsEnum.ToList();
        var connections = new Dictionary<long, PatreonConnection>();
        foreach (var userPatreonConnection in userPatreonConnections)
        {
            var connectionResult = await patreonService.GetPatreonConnection(userPatreonConnection.PatreonConnectionId);
            if (!connectionResult.TryGetDataNonNull(out var connection))
                return Problem(statusCode: connectionResult.GetStatusCodeInt(), detail: connectionResult.ErrorMessage);
            
            connections[userPatreonConnection.PatreonConnectionId] = connection;
        }
        
        var apiModels = userPatreonConnections.Select(model => new ApiPatreonAccount
        {
            UserPatreonConnectionId = model.UserPatreonConnectionId,
            User = user,
            PatreonConnectionId = model.PatreonConnectionId,
            FullName = connections[model.PatreonConnectionId].FullName,
            Pledge = connections[model.PatreonConnectionId].Pledge,
            ConnectedOn = model.ConnectedOn,
            UpdatedOn = connections[model.PatreonConnectionId].UpdatedOn,
            CreatedOn = connections[model.PatreonConnectionId].CreatedOn
        });
        
        return Ok(apiModels);
    }

}