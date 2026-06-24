namespace Tacdent.Data.Repositories.Interfaces;

/// <summary>
/// Coordinates repositories and commits all of their pending changes in a single transaction
/// via <see cref="SaveChangesAsync"/>, giving one commit per application use case.
/// </summary>
public interface IUnitOfWork
{
    IAppointmentRepository Appointments { get; }

    IServiceRepository Services { get; }

    IUserRepository Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
