using Tacdent.Core.DTOs;
using Tacdent.Core.Entities;

namespace Tacdent.Data.Repositories.Interfaces;

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<PagedResult<Appointment>> GetPagedAsync(
        AppointmentQuery query,
        CancellationToken cancellationToken = default);

    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
