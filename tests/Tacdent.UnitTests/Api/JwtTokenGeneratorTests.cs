using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Tacdent.Api.Auth;
using Tacdent.Core.DTOs;
using Tacdent.Core.Entities;

namespace Tacdent.UnitTests.Api;

public class JwtTokenGeneratorTests
{
    [Fact]
    public void GenerateToken_ReturnsValidJwtWithPerUserClaims()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "Tacdent-Test",
            Audience = "Tacdent-Test-Audience",
            Key = "TacDent-Unit-Test-Jwt-Signing-Key-32!!",
            ExpiryMinutes = 60,
        });
        var sut = new JwtTokenGenerator(options);
        var user = new AuthenticatedUserDto(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "admin@tacdent.local",
            UserRole.Admin);
        var before = DateTime.UtcNow;

        var (token, expiresAt) = sut.GenerateToken(user);

        token.ShouldNotBeNullOrWhiteSpace();
        expiresAt.ShouldBeGreaterThan(before);
        expiresAt.ShouldBeLessThanOrEqualTo(before.AddMinutes(60).AddSeconds(1));

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Issuer.ShouldBe("Tacdent-Test");
        jwt.Audiences.ShouldContain("Tacdent-Test-Audience");
        jwt.Claims.ShouldContain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        jwt.Claims.ShouldContain(c => c.Type == ClaimTypes.Name && c.Value == user.Email);
        jwt.Claims.ShouldContain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
        jwt.Claims.ShouldContain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
    }
}
