using Tacdent.Core.DTOs;
using Tacdent.Core.Entities;
using Tacdent.Core.Results;

namespace Tacdent.Application.Services.Interfaces;

public interface IAppointmentService
{
    Task<IReadOnlyList<AppointmentDto>> GetAllAsync(
        AppointmentStatus? status,
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
}
