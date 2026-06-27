using Microsoft.EntityFrameworkCore;
using Tacdent.Core.DTOs;
using Tacdent.Core.Entities;
using Tacdent.Data.Context;
using Tacdent.Data.Repositories.Interfaces;

namespace Tacdent.Data.Repositories;

public class AppointmentRepository(TacdentDbContext context)
    : Repository<Appointment>(context), IAppointmentRepository
{
    public async Task<PagedResult<Appointment>> GetPagedAsync(
        AppointmentQuery query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Appointment> filtered = Set.AsNoTracking();

        if (query.Status.HasValue)
        {
            filtered = filtered.Where(a => a.Status == query.Status.Value);
        }

        var totalCount = await filtered.CountAsync(cancellationToken);

        var items = await ApplySort(filtered, query.SortBy, query.SortDirection)
            .Include(a => a.AssignedUser)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Appointment>(items, query.Page, query.PageSize, totalCount);
    }

    // Tracked lookup (no AsNoTracking) so the returned entity can be updated or removed.
    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await Set
            .Include(a => a.AssignedUser)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    private static IQueryable<Appointment> ApplySort(
        IQueryable<Appointment> query,
        AppointmentSortField sortBy,
        SortDirection sortDirection)
    {
        return (sortBy, sortDirection) switch
        {
            (AppointmentSortField.PreferredDate, SortDirection.Asc) =>
                query.OrderBy(a => a.PreferredDate).ThenBy(a => a.PreferredTime).ThenBy(a => a.Id),
            (AppointmentSortField.PreferredDate, SortDirection.Desc) =>
                query.OrderByDescending(a => a.PreferredDate).ThenBy(a => a.PreferredTime).ThenBy(a => a.Id),
            (AppointmentSortField.CreatedAt, SortDirection.Asc) =>
                query.OrderBy(a => a.CreatedAt).ThenBy(a => a.Id),
            (AppointmentSortField.CreatedAt, SortDirection.Desc) =>
                query.OrderByDescending(a => a.CreatedAt).ThenBy(a => a.Id),
            (AppointmentSortField.Status, SortDirection.Asc) =>
                query.OrderBy(a => a.Status).ThenBy(a => a.Id),
            (AppointmentSortField.Status, SortDirection.Desc) =>
                query.OrderByDescending(a => a.Status).ThenBy(a => a.Id),
            _ => query.OrderByDescending(a => a.PreferredDate).ThenBy(a => a.PreferredTime).ThenBy(a => a.Id)
        };
    }
}
