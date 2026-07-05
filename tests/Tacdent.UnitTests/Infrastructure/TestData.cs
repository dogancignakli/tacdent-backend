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
            ServiceId: 1,
            Notes: "First visit",
            KvkkInformationAccepted: true,
            KvkkInformationVersion: "2026-07-01",
            KvkkExplicitConsentAccepted: true,
            KvkkExplicitConsentVersion: "2026-07-01",
            IpAddress: "127.0.0.1");

    internal static Appointment SampleAppointment(Guid? id = null) =>
        new()
        {
            Id = id ?? Guid.Parse("11111111-1111-1111-1111-111111111111"),
            PatientName = "Jane Doe",
            Email = "jane@example.com",
            Phone = "+15551234567",
            PreferredDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(7)),
            PreferredTime = new TimeOnly(10, 30),
            ServiceId = 1,
            ServiceType = "Genel Muayene",
            Notes = "First visit",
            Status = AppointmentStatus.Pending,
            CreatedAt = AuditTimestamp,
            UpdatedAt = AuditTimestamp,
        };

    internal static DentalService SampleService(int id = 1) =>
        new()
        {
            Id = id,
            NameTr = "Genel Muayene",
            NameEn = "General Checkup",
            DescriptionTr = "Kapsamlı ağız muayenesi.",
            DescriptionEn = "Comprehensive oral exam.",
            Icon = "checkup",
            PriceFromTry = 75m,
            PriceFromEur = 25m,
            DurationMinutes = 45,
            DisplayOrder = 1,
            IsActive = true,
            CreatedAt = AuditTimestamp,
            UpdatedAt = AuditTimestamp,
        };

    internal static User SampleUser(
        string email = "admin@tacdent.local",
        string passwordHash = "hash",
        UserRole role = UserRole.Admin,
        bool isActive = true) =>
        new()
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Email = email,
            PasswordHash = passwordHash,
            Role = role,
            IsActive = isActive,
            CreatedAt = AuditTimestamp,
            UpdatedAt = AuditTimestamp,
        };
}
