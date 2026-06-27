using Riok.Mapperly.Abstractions;
using Tacdent.Core.DTOs;
using Tacdent.Core.Entities;

namespace Tacdent.Application.Mapping;

/// <summary>
/// Compile-time (source-generated) mapper between appointment entities and DTOs.
/// Mapperly generates the implementation of these partial methods at build time — no runtime reflection.
/// </summary>
[Mapper]
public partial class AppointmentMapper
{
    [MapProperty(nameof(Appointment.AssignedUser) + "." + nameof(User.Email), nameof(AppointmentDto.AssignedUserEmail))]
    public partial AppointmentDto ToDto(Appointment entity);

    public partial IReadOnlyList<AppointmentDto> ToDtoList(IReadOnlyList<Appointment> entities);

    [MapperIgnoreTarget(nameof(Appointment.Id))]
    [MapperIgnoreTarget(nameof(Appointment.Status))]
    [MapperIgnoreTarget(nameof(Appointment.CreatedAt))]
    [MapperIgnoreTarget(nameof(Appointment.UpdatedAt))]
    [MapperIgnoreTarget(nameof(Appointment.AssignedUserId))]
    [MapperIgnoreTarget(nameof(Appointment.AssignedUser))]
    public partial Appointment ToEntity(CreateAppointmentDto dto);
}
