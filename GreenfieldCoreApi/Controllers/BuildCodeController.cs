using Asp.Versioning;
using GreenfieldCoreApi.ApiModels;
using GreenfieldCoreServices.Models.BuildCodes;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenfieldCoreApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class BuildCodeController(ICodeService codeService) : ControllerBase
{
    [HttpGet("all")]
    [Authorize(Roles = "Codes.Read,Codes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(IEnumerable<BuildCode>))]
    public async Task<IActionResult> GetAll()
    {
        var codesResult = await codeService.GetAllBuildCodes();
        return codesResult.IsSuccessful
            ? Ok(codesResult.GetNonNullOrThrow())
            : Problem(statusCode: codesResult.GetStatusCodeInt(), detail: codesResult.ErrorMessage);
    }

    [HttpGet("{id:long}")]
    [Authorize(Roles = "Codes.Read,Codes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(BuildCode))]
    public async Task<IActionResult> GetById([FromRoute] long id)
    {
        var codeResult = await codeService.GetBuildCodeById(id);
        return codeResult.IsSuccessful
            ? Ok(codeResult.GetNonNullOrThrow())
            : Problem(statusCode: codeResult.GetStatusCodeInt(), detail: codeResult.ErrorMessage);
    }

    [HttpPost]
    [Authorize(Roles = "Codes.Write,Codes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces(typeof(BuildCode))]
    public async Task<IActionResult> Create([FromBody] BuildCodeRequest request)
    {
        var createdResult = await codeService.CreateBuildCode(request.ListOrder, request.Code);
        if (!createdResult.IsSuccessful)
            return Problem(statusCode: createdResult.GetStatusCodeInt(), detail: createdResult.ErrorMessage);
        var created = createdResult.GetNonNullOrThrow();
        return CreatedAtAction(nameof(GetById), new { version = HttpContext.GetRequestedApiVersion()?.ToString(), id = created.CodeId }, created);
    }

    [HttpPatch("{id:long}")]
    [Authorize(Roles = "Codes.Write,Codes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(BuildCode))]
    public async Task<IActionResult> Update([FromRoute] long id, int? listOrder = null, string? code = null)
    {
        var updatedResult = await codeService.UpdateBuildCode(id, listOrder, code);
        return updatedResult.IsSuccessful
            ? Ok(updatedResult.GetNonNullOrThrow())
            : Problem(statusCode: updatedResult.GetStatusCodeInt(), detail: updatedResult.ErrorMessage);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Codes.Write,Codes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(BuildCode))]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        var deletedResult = await codeService.DeleteBuildCode(id);
        return deletedResult.IsSuccessful
            ? Ok(deletedResult.GetNonNullOrThrow())
            : Problem(statusCode: deletedResult.GetStatusCodeInt(), detail: deletedResult.ErrorMessage);
    }
}