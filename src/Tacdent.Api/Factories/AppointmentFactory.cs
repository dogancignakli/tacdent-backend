using Tacdent.Api.ViewModels;
using Tacdent.Core.DTOs;

namespace Tacdent.Api.Factories;

public class AppointmentFactory : IAppointmentFactory
{
    public CreateAppointmentDto ToCreateDto(CreateAppointmentRequest request) =>
        new(
            request.PatientName.Trim(),
            request.Email.Trim(),
            request.Phone.Trim(),
            request.PreferredDate,
            request.PreferredTime,
            request.ServiceType.Trim(),
            string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim());

    public UpdateAppointmentStatusDto ToUpdateStatusDto(UpdateAppointmentStatusRequest request) =>
        new(request.Status);

    public AssignAppointmentDto ToAssignDto(AssignAppointmentRequest request) =>
        new(request.AssignedUserId);

    public AppointmentQuery ToQuery(AppointmentQueryRequest request) =>
        new(request.Status, request.Page, request.PageSize, request.SortBy, request.SortDirection);

    public AppointmentResponse ToResponse(AppointmentDto dto) =>
        new(
            dto.Id,
            dto.PatientName,
            dto.Email,
            dto.Phone,
            dto.PreferredDate,
            dto.PreferredTime,
            dto.ServiceType,
            dto.Notes,
            dto.Status,
            dto.CreatedAt,
            dto.UpdatedAt,
            dto.AssignedUserId,
            dto.AssignedUserEmail);
}
