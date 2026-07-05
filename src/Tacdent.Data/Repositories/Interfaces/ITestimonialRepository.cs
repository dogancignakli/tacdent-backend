using Tacdent.Core.Entities;

namespace Tacdent.Data.Repositories.Interfaces;

public interface ITestimonialRepository : IRepository<Testimonial>
{
    Task<IReadOnlyList<Testimonial>> GetActiveAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Testimonial>> GetAllOrderedAsync(CancellationToken cancellationToken = default);
}
