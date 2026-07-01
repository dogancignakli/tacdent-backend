using FluentValidation.TestHelper;
using Tacdent.Api.Validators;
using Tacdent.Api.ViewModels;

namespace Tacdent.UnitTests.Api;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _sut = new();

    [Fact]
    public async Task Validate_WhenEmailAndPasswordProvided_ShouldNotHaveErrors()
    {
        var request = new LoginRequest
        {
            Email = "admin@tacdent.local",
            Password = "secret",
            RecaptchaToken = "test-token",
        };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WhenEmailIsEmpty_ShouldHaveValidationError()
    {
        var request = new LoginRequest { Email = string.Empty, Password = "secret" };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_WhenEmailIsInvalid_ShouldHaveValidationError()
    {
        var request = new LoginRequest { Email = "not-an-email", Password = "secret" };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_WhenPasswordIsEmpty_ShouldHaveValidationError()
    {
        var request = new LoginRequest { Email = "admin@tacdent.local", Password = string.Empty };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
