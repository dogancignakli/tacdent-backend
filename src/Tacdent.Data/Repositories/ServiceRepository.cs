using Microsoft.EntityFrameworkCore;
using Tacdent.Core.Entities;
using Tacdent.Data.Context;
using Tacdent.Data.Repositories.Interfaces;

namespace Tacdent.Data.Repositories;

public class ServiceRepository(TacdentDbContext context)
    : Repository<DentalService>(context), IServiceRepository
{
    public async Task<IReadOnlyList<DentalService>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await Set.AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.NameTr)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<DentalService>> GetAllOrderedAsync(CancellationToken cancellationToken = default)
        => await Set.AsNoTracking()
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.NameTr)
            .ToListAsync(cancellationToken);

    public Task<bool> HasAppointmentsAsync(int serviceId, CancellationToken cancellationToken = default)
        => Context.Appointments.AnyAsync(a => a.ServiceId == serviceId, cancellationToken);
}
