using Asp.Versioning;
using GreenfieldCoreModels.ApiModels.User;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenfieldCoreApi.Controllers;

[ApiController]
// [Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class UserController(IUserService userService) : ControllerBase
{
    // [Authorize(Roles = "Admin")]
    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<UserResponse>> CreateUser(Guid minecraftUuid, string minecraftUsername,  string? displayName)
    {
        var user = await userService.CreateUser(minecraftUuid, minecraftUsername, displayName);
        
        if (!user.Success) return BadRequest(user.Message);
        
        return Created($"/api/v1/user/{user.Data!.MinecraftUuid}", new UserResponse
        {
            MinecraftUuid = user.Data.MinecraftUuid,
            Username = user.Data.Username,
            DisplayName = user.Data.DisplayName
        });
    }

    // [Authorize(Roles = "Admin")]
    [HttpGet("{minecraftUuid:guid}")]
    public async Task<ActionResult<UserResponse>> GetUser(Guid minecraftUuid)
    {
        var user = await userService.GetUser(minecraftUuid);

        if (!user.Success) return BadRequest(user.Message);
        
        return Ok(new UserResponse
        {
            MinecraftUuid = user.Data!.MinecraftUuid,
            Username = user.Data.Username,
            DisplayName = user.Data.DisplayName
        });
    }
    
    // [Authorize(Roles = "Admin")]
    [HttpGet("find/{username}")]
    public async Task<ActionResult<IEnumerable<UserResponse>>> FindUser(string username)
    {
        var users = await userService.FindUser(username);

        if (!users.Success) return BadRequest(users.Message);
        
        return Ok(users.Data.Select(user => new UserResponse
        {
            MinecraftUuid = user.MinecraftUuid,
            Username = user.Username,
            DisplayName = user.DisplayName
        }));
    }
    
}