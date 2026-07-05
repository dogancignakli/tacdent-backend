namespace Tacdent.Core.DTOs;

/// <summary>Input contract consumed by the Application layer to create an appointment.</summary>
public record CreateAppointmentDto(
    string PatientName,
    string Email,
    string Phone,
    DateOnly PreferredDate,
    TimeOnly PreferredTime,
    int ServiceId,
    string? Notes,
    bool KvkkInformationAccepted,
    string KvkkInformationVersion,
    bool KvkkExplicitConsentAccepted,
    string KvkkExplicitConsentVersion,
    string? IpAddress
);
