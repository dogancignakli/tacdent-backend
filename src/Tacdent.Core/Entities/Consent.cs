namespace Tacdent.Core.Entities;

public class Consent
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public ConsentType ConsentType { get; set; }
    public required string TextVersion { get; set; }
    public DateTime AcceptedAt { get; set; }
    public required string PatientName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public string? IpAddress { get; set; }

    public Appointment Appointment { get; set; } = null!;
}
