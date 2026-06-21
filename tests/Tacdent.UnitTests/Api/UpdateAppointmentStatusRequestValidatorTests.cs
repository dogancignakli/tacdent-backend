using FluentValidation.TestHelper;
using Tacdent.Api.Validators;
using Tacdent.Api.ViewModels;
using Tacdent.Core.Entities;

namespace Tacdent.UnitTests.Api;

public class UpdateAppointmentStatusRequestValidatorTests
{
    private readonly UpdateAppointmentStatusRequestValidator _sut = new();

    [Fact]
    public async Task Validate_WhenStatusIsValid_ShouldNotHaveErrors()
    {
        var request = new UpdateAppointmentStatusRequest { Status = AppointmentStatus.Confirmed };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WhenStatusIsInvalid_ShouldHaveValidationError()
    {
        var request = new UpdateAppointmentStatusRequest { Status = (AppointmentStatus)999 };

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Status);
    }
}
