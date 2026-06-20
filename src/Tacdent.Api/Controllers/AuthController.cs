using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tacdent.Api.Auth;
using Tacdent.Api.Extensions;
using Tacdent.Api.ViewModels;
using Tacdent.Application.Services.Interfaces;

namespace Tacdent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, IJwtTokenGenerator jwtTokenGenerator) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var result = authService.Authenticate(request.Password);

        if (result.IsFailure)
        {
            return result.Error.ToProblemResult();
        }

        var (token, expiresAt) = jwtTokenGenerator.GenerateToken();
        return Ok(new LoginResponse(token, expiresAt));
    }
}
