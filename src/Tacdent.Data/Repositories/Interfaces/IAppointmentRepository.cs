using Tacdent.Core.Entities;

namespace Tacdent.Data.Repositories.Interfaces;

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<IReadOnlyList<Appointment>> GetAllAsync(
        AppointmentStatus? status,
        CancellationToken cancellationToken = default);

    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
