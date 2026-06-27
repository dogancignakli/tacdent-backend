using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tacdent.Api.Auth;
using Tacdent.Api.Extensions;
using Tacdent.Api.Factories;
using Tacdent.Api.ViewModels;
using Tacdent.Application.Services.Interfaces;

namespace Tacdent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin)]
public class UsersController(IUserManagementService userManagementService, IUserFactory factory) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await userManagementService.GetAllAsync(cancellationToken);
        return Ok(users.Select(factory.ToResponse));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await userManagementService.CreateAsync(factory.ToCreateDto(request), cancellationToken);
        return result.ToCreatedResult(factory.ToResponse);
    }

    [HttpPatch("{id:guid}/role")]
    public async Task<IActionResult> UpdateRole(
        Guid id,
        [FromBody] UpdateUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await userManagementService.UpdateRoleAsync(
            id,
            factory.ToUpdateRoleDto(request),
            cancellationToken);

        return result.ToOkResult(factory.ToResponse);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> SetActive(
        Guid id,
        [FromBody] UpdateUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await userManagementService.SetActiveAsync(
            id,
            factory.ToUpdateStatusDto(request),
            cancellationToken);

        return result.ToOkResult(factory.ToResponse);
    }

    [HttpPost("{id:guid}/password")]
    public async Task<IActionResult> ResetPassword(
        Guid id,
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await userManagementService.ResetPasswordAsync(
            id,
            factory.ToResetPasswordDto(request),
            cancellationToken);

        return result.ToNoContentResult();
    }
}
