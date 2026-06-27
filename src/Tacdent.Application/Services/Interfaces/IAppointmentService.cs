using Tacdent.Core.DTOs;
using Tacdent.Core.Entities;
using Tacdent.Core.Results;

namespace Tacdent.Application.Services.Interfaces;

public interface IAppointmentService
{
    Task<PagedResult<AppointmentDto>> GetPagedAsync(
        AppointmentQuery query,
        CancellationToken cancellationToken = default);

    Task<Result<AppointmentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<AppointmentDto>> CreateAsync(
        CreateAppointmentDto dto,
        CancellationToken cancellationToken = default);

    Task<Result<AppointmentDto>> UpdateStatusAsync(
        Guid id,
        UpdateAppointmentStatusDto dto,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<AppointmentDto>> AssignAsync(
        Guid id,
        AssignAppointmentDto dto,
        CancellationToken cancellationToken = default);
}
