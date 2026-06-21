using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Tacdent.Api.Auth;

namespace Tacdent.UnitTests.Api;

public class JwtTokenGeneratorTests
{
    [Fact]
    public void GenerateToken_ReturnsValidJwtWithExpectedClaims()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "Tacdent-Test",
            Audience = "Tacdent-Test-Audience",
            Key = "TacDent-Unit-Test-Jwt-Signing-Key-32!!",
            ExpiryMinutes = 60,
        });
        var sut = new JwtTokenGenerator(options);
        var before = DateTime.UtcNow;

        var (token, expiresAt) = sut.GenerateToken();

        token.ShouldNotBeNullOrWhiteSpace();
        expiresAt.ShouldBeGreaterThan(before);
        expiresAt.ShouldBeLessThanOrEqualTo(before.AddMinutes(60).AddSeconds(1));

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Issuer.ShouldBe("Tacdent-Test");
        jwt.Audiences.ShouldContain("Tacdent-Test-Audience");
        jwt.Claims.ShouldContain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        jwt.Claims.ShouldContain(c => c.Type == ClaimTypes.Name && c.Value == "admin");
    }
}
