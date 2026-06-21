using FluentValidation.TestHelper;
using Tacdent.Api.Validators;
using Tacdent.Api.ViewModels;

namespace Tacdent.UnitTests.Api;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _sut = new();

    [Fact]
    public async Task Validate_WhenPasswordIsProvided_ShouldNotHaveErrors()
    {
        var request = new LoginRequest { Password = "secret" };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WhenPasswordIsEmpty_ShouldHaveValidationError()
    {
        var request = new LoginRequest { Password = string.Empty };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
