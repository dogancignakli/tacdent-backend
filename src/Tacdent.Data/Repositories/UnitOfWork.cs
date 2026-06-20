using Tacdent.Data.Context;
using Tacdent.Data.Repositories.Interfaces;

namespace Tacdent.Data.Repositories;

public class UnitOfWork(
    TacdentDbContext context,
    IAppointmentRepository appointments,
    IServiceRepository services) : IUnitOfWork
{
    public IAppointmentRepository Appointments { get; } = appointments;

    public IServiceRepository Services { get; } = services;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}
