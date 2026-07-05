using Tacdent.Data.Context;
using Tacdent.Data.Repositories.Interfaces;

namespace Tacdent.Data.Repositories;

public class UnitOfWork(
    TacdentDbContext context,
    IAppointmentRepository appointments,
    IServiceRepository services,
    IUserRepository users,
    ITestimonialRepository testimonials,
    IConsentRepository consents) : IUnitOfWork
{
    public IAppointmentRepository Appointments { get; } = appointments;

    public IServiceRepository Services { get; } = services;

    public IUserRepository Users { get; } = users;

    public ITestimonialRepository Testimonials { get; } = testimonials;

    public IConsentRepository Consents { get; } = consents;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
