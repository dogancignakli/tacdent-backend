using Tacdent.Core.Entities;

namespace Tacdent.Data.Repositories.Interfaces;

public interface IServiceRepository : IRepository<DentalService>
{
    Task<IReadOnlyList<DentalService>> GetActiveAsync(CancellationToken cancellationToken = default);
}
