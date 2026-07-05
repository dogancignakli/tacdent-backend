using Tacdent.Core.Entities;

namespace Tacdent.Data.Repositories.Interfaces;

public interface IServiceRepository : IRepository<DentalService>
{
    Task<IReadOnlyList<DentalService>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DentalService>> GetAllOrderedAsync(CancellationToken cancellationToken = default);

    Task<bool> HasAppointmentsAsync(int serviceId, CancellationToken cancellationToken = default);
}
