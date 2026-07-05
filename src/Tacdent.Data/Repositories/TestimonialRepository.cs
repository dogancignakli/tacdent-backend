using Microsoft.EntityFrameworkCore;
using Tacdent.Core.Entities;
using Tacdent.Data.Context;
using Tacdent.Data.Repositories.Interfaces;

namespace Tacdent.Data.Repositories;

public class TestimonialRepository(TacdentDbContext context)
    : Repository<Testimonial>(context), ITestimonialRepository
{
    public async Task<IReadOnlyList<Testimonial>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await Set.AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.DisplayOrder)
            .ThenBy(t => t.AuthorName)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Testimonial>> GetAllOrderedAsync(CancellationToken cancellationToken = default)
        => await Set.AsNoTracking()
            .OrderBy(t => t.DisplayOrder)
            .ThenBy(t => t.AuthorName)
            .ToListAsync(cancellationToken);
}
