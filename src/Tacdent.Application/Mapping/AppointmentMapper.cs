using Riok.Mapperly.Abstractions;
using Tacdent.Core.DTOs;
using Tacdent.Core.Entities;

namespace Tacdent.Application.Mapping;

[Mapper]
public partial class AppointmentMapper
{
    [MapProperty(nameof(Appointment.AssignedUser) + "." + nameof(User.Email), nameof(AppointmentDto.AssignedUserEmail))]
    [MapperIgnoreSource(nameof(Appointment.ServiceId))]
    [MapperIgnoreSource(nameof(Appointment.Service))]
    [MapperIgnoreSource(nameof(Appointment.Consents))]
    public partial AppointmentDto ToDto(Appointment entity);

    public partial IReadOnlyList<AppointmentDto> ToDtoList(IReadOnlyList<Appointment> entities);
}
