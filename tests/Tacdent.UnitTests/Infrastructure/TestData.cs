using Tacdent.Core.DTOs;
using Tacdent.Core.Entities;

namespace Tacdent.UnitTests.Infrastructure;

internal static class TestData
{
    internal static readonly DateTime AuditTimestamp = new(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);

    internal static CreateAppointmentDto ValidCreateDto() =>
        new(
            "Jane Doe",
            "jane@example.com",
            "+15551234567",
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(7)),
            new TimeOnly(10, 30),
            "General Checkup",
            "First visit");

    internal static Appointment SampleAppointment(Guid? id = null) =>
        new()
        {
            Id = id ?? Guid.Parse("11111111-1111-1111-1111-111111111111"),
            PatientName = "Jane Doe",
            Email = "jane@example.com",
            Phone = "+15551234567",
            PreferredDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(7)),
            PreferredTime = new TimeOnly(10, 30),
            ServiceType = "General Checkup",
            Notes = "First visit",
            Status = AppointmentStatus.Pending,
            CreatedAt = AuditTimestamp,
            UpdatedAt = AuditTimestamp,
        };

    internal static DentalService SampleService(int id = 1) =>
        new()
        {
            Id = id,
            Name = "General Checkup",
            Description = "Comprehensive oral exam.",
            Icon = "checkup",
            PriceFrom = 75m,
            DurationMinutes = 45,
            IsActive = true,
            CreatedAt = AuditTimestamp,
            UpdatedAt = AuditTimestamp,
        };
}
