using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Tacdent.Api.Auth;
using Tacdent.Api.Extensions;
using Tacdent.Api.ViewModels;
using Tacdent.Application.Services.Interfaces;

namespace Tacdent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IAuthService authService,
    IJwtTokenGenerator jwtTokenGenerator,
    IRecaptchaValidator recaptchaValidator) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var recaptchaResult = await recaptchaValidator.ValidateAsync(
            request.RecaptchaToken,
            "login",
            HttpContext.GetClientIpAddress(),
            cancellationToken);

        if (recaptchaResult.IsFailure)
        {
            return recaptchaResult.Error.ToProblemResult();
        }

        var result = await authService.AuthenticateAsync(request.Email, request.Password, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToProblemResult();
        }

        var (token, expiresAt) = jwtTokenGenerator.GenerateToken(result.Value);
        return Ok(new LoginResponse(token, expiresAt, result.Value.Role.ToString()));
    }
}
