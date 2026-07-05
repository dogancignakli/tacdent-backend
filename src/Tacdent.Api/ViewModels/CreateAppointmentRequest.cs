namespace Tacdent.Api.ViewModels;

/// <summary>Shape of the JSON body the frontend sends to create an appointment.</summary>
public class CreateAppointmentRequest
{
    public string PatientName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateOnly PreferredDate { get; set; }
    public TimeOnly PreferredTime { get; set; }
    public int ServiceId { get; set; }
    public string? Notes { get; set; }
    public bool KvkkInformationAccepted { get; set; }
    public string KvkkInformationVersion { get; set; } = string.Empty;
    public bool KvkkExplicitConsentAccepted { get; set; }
    public string KvkkExplicitConsentVersion { get; set; } = string.Empty;
    public string RecaptchaToken { get; set; } = string.Empty;
}
