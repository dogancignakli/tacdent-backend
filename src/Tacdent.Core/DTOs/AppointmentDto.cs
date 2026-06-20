using Tacdent.Core.Entities;

namespace Tacdent.Core.DTOs;

/// <summary>Output contract returned by the Application layer for an appointment.</summary>
public record AppointmentDto(
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
    DateTime UpdatedAt
);
