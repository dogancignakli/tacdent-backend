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

        RuleFor(x => x.PreferredDate)
            .Must(date => date.DayOfWeek != DayOfWeek.Sunday)
            .WithMessage("The clinic is closed on Sundays. Please choose another day.");

        RuleFor(x => x.PreferredTime)
            .Must((request, time) => IsWithinWorkingHours(request.PreferredDate, time))
            .When(x => x.PreferredDate.DayOfWeek != DayOfWeek.Sunday)
            .WithMessage(
                "Preferred time is outside working hours (Mon-Sat 09:00-18:00) and must fall on a 30-minute slot.");

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

    // Muayenehane calisma saatleri: Pzt-Cmt 09:00-18:00, Pazar kapali.
    // 30 dakikalik slotlar; kapanis saati haric (son slot kapanistan 30 dk once).
    private static bool IsWithinWorkingHours(DateOnly date, TimeOnly time)
    {
        if (date.DayOfWeek == DayOfWeek.Sunday)
        {
            return false;
        }

        var open = new TimeOnly(9, 0);
        var close = new TimeOnly(18, 0);

        if (time < open || time >= close)
        {
            return false;
        }

        return time is { Minute: 0 or 30, Second: 0 };
    }
}
