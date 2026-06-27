using Tacdent.Core.Entities.Common;

namespace Tacdent.Core.Entities;

public class Appointment : AuditableEntity
{
    public Guid Id { get; set; }
    public required string PatientName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public DateOnly PreferredDate { get; set; }
    public TimeOnly PreferredTime { get; set; }
    public required string ServiceType { get; set; }
    public string? Notes { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    public Guid? AssignedUserId { get; set; }

    public User? AssignedUser { get; set; }
}
