using FluentValidation;
using Tacdent.Api.ViewModels;

namespace Tacdent.Api.Validators;

public class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
{
    public CreateAppointmentRequestValidator()
    {
        RuleFor(x => x.PatientName)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(x => x.ServiceId)
            .GreaterThan(0);

        RuleFor(x => x.Notes)
            .MaximumLength(1000);

        RuleFor(x => x.PreferredDate)
            .GreaterThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Preferred date cannot be in the past.");

        RuleFor(x => x.KvkkInformationAccepted)
            .Equal(true)
            .WithMessage("KVKK information notice must be accepted.");

        RuleFor(x => x.KvkkExplicitConsentAccepted)
            .Equal(true)
            .WithMessage("KVKK explicit consent must be accepted.");

        RuleFor(x => x.KvkkInformationVersion)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.KvkkExplicitConsentVersion)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.RecaptchaToken).NotEmpty();
    }
}
