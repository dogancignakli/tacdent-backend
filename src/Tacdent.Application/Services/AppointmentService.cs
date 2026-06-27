using Tacdent.Application.Errors;
using Tacdent.Application.Mapping;
using Tacdent.Application.Services.Interfaces;
using Tacdent.Core.DTOs;
using Tacdent.Core.Entities;
using Tacdent.Core.Results;
using Tacdent.Data.Repositories.Interfaces;

namespace Tacdent.Application.Services;

public class AppointmentService(IUnitOfWork unitOfWork, AppointmentMapper mapper) : IAppointmentService
{
    public async Task<PagedResult<AppointmentDto>> GetPagedAsync(
        AppointmentQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = await unitOfWork.Appointments.GetPagedAsync(query, cancellationToken);
        return new PagedResult<AppointmentDto>(
            mapper.ToDtoList(page.Items),
            page.Page,
            page.PageSize,
            page.TotalCount);
    }

    public async Task<Result<AppointmentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var appointment = await unitOfWork.Appointments.GetByIdAsync(id, cancellationToken);

        return appointment is null
            ? Result.Failure<AppointmentDto>(AppointmentErrors.NotFound(id))
            : Result.Success(mapper.ToDto(appointment));
    }

    public async Task<Result<AppointmentDto>> CreateAsync(
        CreateAppointmentDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.PreferredDate < DateOnly.FromDateTime(DateTime.UtcNow.Date))
        {
            return Result.Failure<AppointmentDto>(AppointmentErrors.PastDate);
        }

        var appointment = mapper.ToEntity(dto);
        appointment.Id = Guid.NewGuid();
        appointment.Status = AppointmentStatus.Pending;

        await unitOfWork.Appointments.AddAsync(appointment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.ToDto(appointment));
    }

    public async Task<Result<AppointmentDto>> UpdateStatusAsync(
        Guid id,
        UpdateAppointmentStatusDto dto,
        CancellationToken cancellationToken = default)
    {
        var appointment = await unitOfWork.Appointments.GetByIdAsync(id, cancellationToken);
        if (appointment is null)
        {
            return Result.Failure<AppointmentDto>(AppointmentErrors.NotFound(id));
        }

        appointment.Status = dto.Status;
        unitOfWork.Appointments.Update(appointment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.ToDto(appointment));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var appointment = await unitOfWork.Appointments.GetByIdAsync(id, cancellationToken);
        if (appointment is null)
        {
            return Result.Failure(AppointmentErrors.NotFound(id));
        }

        unitOfWork.Appointments.Remove(appointment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<AppointmentDto>> AssignAsync(
        Guid id,
        AssignAppointmentDto dto,
        CancellationToken cancellationToken = default)
    {
        var appointment = await unitOfWork.Appointments.GetByIdAsync(id, cancellationToken);
        if (appointment is null)
        {
            return Result.Failure<AppointmentDto>(AppointmentErrors.NotFound(id));
        }

        if (dto.AssignedUserId is null)
        {
            appointment.AssignedUserId = null;
            appointment.AssignedUser = null;
        }
        else
        {
            var assignee = await unitOfWork.Users.GetByIdAsync(dto.AssignedUserId.Value, cancellationToken);
            if (assignee is null)
            {
                return Result.Failure<AppointmentDto>(UserErrors.AssigneeNotFound(dto.AssignedUserId.Value));
            }

            if (!assignee.IsActive)
            {
                return Result.Failure<AppointmentDto>(UserErrors.InactiveAssignee);
            }

            appointment.AssignedUserId = assignee.Id;
            appointment.AssignedUser = assignee;
        }

        unitOfWork.Appointments.Update(appointment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(mapper.ToDto(appointment));
    }
}
