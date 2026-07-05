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

        if (!dto.KvkkInformationAccepted || !dto.KvkkExplicitConsentAccepted)
        {
            return Result.Failure<AppointmentDto>(AppointmentErrors.ConsentRequired);
        }

        if (string.IsNullOrWhiteSpace(dto.KvkkInformationVersion)
            || string.IsNullOrWhiteSpace(dto.KvkkExplicitConsentVersion))
        {
            return Result.Failure<AppointmentDto>(AppointmentErrors.ConsentRequired);
        }

        var service = await unitOfWork.Services.GetByIdAsync(dto.ServiceId, cancellationToken);
        if (service is null || !service.IsActive)
        {
            return Result.Failure<AppointmentDto>(AppointmentErrors.InvalidService);
        }

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientName = dto.PatientName.Trim(),
            Email = dto.Email.Trim(),
            Phone = dto.Phone.Trim(),
            PreferredDate = dto.PreferredDate,
            PreferredTime = dto.PreferredTime,
            ServiceId = service.Id,
            ServiceType = service.NameTr,
            Notes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim(),
            Status = AppointmentStatus.Pending,
        };

        var acceptedAt = DateTime.UtcNow;
        var consents = new[]
        {
            new Consent
            {
                Id = Guid.NewGuid(),
                AppointmentId = appointment.Id,
                ConsentType = ConsentType.KvkkInformation,
                TextVersion = dto.KvkkInformationVersion.Trim(),
                AcceptedAt = acceptedAt,
                PatientName = appointment.PatientName,
                Email = appointment.Email,
                Phone = appointment.Phone,
                IpAddress = dto.IpAddress,
            },
            new Consent
            {
                Id = Guid.NewGuid(),
                AppointmentId = appointment.Id,
                ConsentType = ConsentType.KvkkExplicitConsent,
                TextVersion = dto.KvkkExplicitConsentVersion.Trim(),
                AcceptedAt = acceptedAt,
                PatientName = appointment.PatientName,
                Email = appointment.Email,
                Phone = appointment.Phone,
                IpAddress = dto.IpAddress,
            },
        };

        await unitOfWork.Appointments.AddAsync(appointment, cancellationToken);
        foreach (var consent in consents)
        {
            await unitOfWork.Consents.AddAsync(consent, cancellationToken);
        }

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
