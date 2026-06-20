using Tacdent.Core.Results;

namespace Tacdent.Application.Errors;

public static class AppointmentErrors
{
    public static readonly Error PastDate = Error.Validation(
        "Appointment.PastDate",
        "Preferred date cannot be in the past.");

    public static Error NotFound(Guid id) => Error.NotFound(
        "Appointment.NotFound",
        $"Appointment '{id}' was not found.");
}
