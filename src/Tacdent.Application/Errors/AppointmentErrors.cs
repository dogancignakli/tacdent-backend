using Tacdent.Core.Results;

namespace Tacdent.Application.Errors;

public static class AppointmentErrors
{
    public static readonly Error PastDate = Error.Validation(
        "Appointment.PastDate",
        "Preferred date cannot be in the past.");

    public static readonly Error ConsentRequired = Error.Validation(
        "Appointment.ConsentRequired",
        "KVKK information notice and explicit consent are required.");

    public static readonly Error InvalidService = Error.Validation(
        "Appointment.InvalidService",
        "Selected service is not available.");

    public static Error NotFound(Guid id) => Error.NotFound(
        "Appointment.NotFound",
        $"Appointment '{id}' was not found.");
}
