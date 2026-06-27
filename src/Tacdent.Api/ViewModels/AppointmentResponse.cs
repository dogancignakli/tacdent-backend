using Tacdent.Core.Entities;

namespace Tacdent.Api.ViewModels;

/// <summary>Shape of the JSON returned to the frontend for an appointment.</summary>
public record AppointmentResponse(
    Guid Id,
    string PatientName,
    string Email,
    string Phone,
    DateOnly PreferredDate,
    TimeOnly PreferredTime,
    string ServiceType,
    string? Notes,
    AppointmentStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid? AssignedUserId,
    string? AssignedUserEmail
);
