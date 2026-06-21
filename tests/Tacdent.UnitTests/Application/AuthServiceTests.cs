using Microsoft.Extensions.Options;
using Tacdent.Application.Errors;
using Tacdent.Application.Options;
using Tacdent.Application.Services;

namespace Tacdent.UnitTests.Application;

public class AuthServiceTests
{
    private const string Password = "TacDent-Dev-Admin-2026!";

    private static AuthService CreateSut(string password = Password) =>
        new(Options.Create(new AuthOptions { AdminPassword = password }));

    [Fact]
    public void Authenticate_WhenPasswordMatches_ReturnsSuccess()
    {
        var result = CreateSut().Authenticate(Password);

        result.IsSuccess.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("wrong-password")]
    [InlineData("tacdent-dev-admin-2026!")]
    public void Authenticate_WhenPasswordDoesNotMatch_ReturnsInvalidCredentials(string password)
    {
        var result = CreateSut().Authenticate(password);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(AuthErrors.InvalidCredentials.Code);
    }
}
