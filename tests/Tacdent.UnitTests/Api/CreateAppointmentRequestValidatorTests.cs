using FluentValidation.TestHelper;
using Tacdent.Api.Validators;
using Tacdent.Api.ViewModels;

namespace Tacdent.UnitTests.Api;

public class CreateAppointmentRequestValidatorTests
{
    private readonly CreateAppointmentRequestValidator _sut = new();

    private static CreateAppointmentRequest ValidRequest() =>
        new()
        {
            PatientName = "Jane Doe",
            Email = "jane@example.com",
            Phone = "+15551234567",
            PreferredDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(3)),
            PreferredTime = new TimeOnly(10, 30),
            ServiceId = 1,
            Notes = "First visit",
            KvkkInformationAccepted = true,
            KvkkInformationVersion = "2026-07-01",
            KvkkExplicitConsentAccepted = true,
            KvkkExplicitConsentVersion = "2026-07-01",
            RecaptchaToken = "test-token",
        };

    [Fact]
    public async Task Validate_WhenRequestIsValid_ShouldNotHaveErrors()
    {
        var result = await _sut.TestValidateAsync(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_WhenPatientNameIsEmpty_ShouldHaveValidationError()
    {
        var request = ValidRequest();
        request.PatientName = string.Empty;

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.PatientName);
    }

    [Fact]
    public async Task Validate_WhenEmailIsInvalid_ShouldHaveValidationError()
    {
        var request = ValidRequest();
        request.Email = "not-an-email";

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_WhenPreferredDateIsInPast_ShouldHaveValidationError()
    {
        var request = ValidRequest();
        request.PreferredDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1));

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.PreferredDate);
    }

    [Fact]
    public async Task Validate_WhenNotesExceedMaxLength_ShouldHaveValidationError()
    {
        var request = ValidRequest();
        request.Notes = new string('x', 1001);

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public async Task Validate_WhenKvkkConsentsNotAccepted_ShouldHaveValidationErrors()
    {
        var request = ValidRequest();
        request.KvkkInformationAccepted = false;
        request.KvkkExplicitConsentAccepted = false;

        var result = await _sut.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.KvkkInformationAccepted);
        result.ShouldHaveValidationErrorFor(x => x.KvkkExplicitConsentAccepted);
    }
}
