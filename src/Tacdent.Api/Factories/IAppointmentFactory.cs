using Tacdent.Api.ViewModels;
using Tacdent.Core.DTOs;

namespace Tacdent.Api.Factories;

/// <summary>
/// Converts between Api ViewModels (the HTTP contract) and Core DTOs (the Application contract),
/// keeping controllers free of mapping logic.
/// </summary>
public interface IAppointmentFactory
{
    CreateAppointmentDto ToCreateDto(CreateAppointmentRequest request);

    UpdateAppointmentStatusDto ToUpdateStatusDto(UpdateAppointmentStatusRequest request);

    AppointmentResponse ToResponse(AppointmentDto dto);
}
