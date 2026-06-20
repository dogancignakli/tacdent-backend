namespace Tacdent.Data.Repositories.Interfaces;

/// <summary>
/// Generic repository abstraction for basic persistence operations.
/// Implementations do not call SaveChanges; committing is the Unit of Work's responsibility.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    void Update(T entity);

    void Remove(T entity);
}
