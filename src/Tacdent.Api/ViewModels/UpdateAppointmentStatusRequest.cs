using Tacdent.Core.Entities;

namespace Tacdent.Api.ViewModels;

public class UpdateAppointmentStatusRequest
{
    public AppointmentStatus Status { get; set; }
}
