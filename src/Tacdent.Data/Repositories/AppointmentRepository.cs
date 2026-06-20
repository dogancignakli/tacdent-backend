using Microsoft.EntityFrameworkCore;
using Tacdent.Core.Entities;
using Tacdent.Data.Context;
using Tacdent.Data.Repositories.Interfaces;

namespace Tacdent.Data.Repositories;

public class AppointmentRepository(TacdentDbContext context)
    : Repository<Appointment>(context), IAppointmentRepository
{
    public async Task<IReadOnlyList<Appointment>> GetAllAsync(
        AppointmentStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        return await query
            .OrderByDescending(a => a.PreferredDate)
            .ThenBy(a => a.PreferredTime)
            .ToListAsync(cancellationToken);
    }

    // Tracked lookup (no AsNoTracking) so the returned entity can be updated or removed.
    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await Set.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
}
