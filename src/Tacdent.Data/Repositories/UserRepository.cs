using Microsoft.EntityFrameworkCore;
using Tacdent.Core.Entities;
using Tacdent.Data.Context;
using Tacdent.Data.Repositories.Interfaces;

namespace Tacdent.Data.Repositories;

public class UserRepository(TacdentDbContext context)
    : Repository<User>(context), IUserRepository
{
    // Tracked lookup (no AsNoTracking) so failure/lockout counters can be updated and saved.
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await Set.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await Set.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<IReadOnlyList<User>> GetAllOrderedAsync(CancellationToken cancellationToken = default)
        => await Set
            .AsNoTracking()
            .OrderBy(u => u.Email)
            .ToListAsync(cancellationToken);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        => await Set.AnyAsync(u => u.Email == email, cancellationToken);

    public async Task<int> CountActiveAdminsAsync(CancellationToken cancellationToken = default)
        => await Set.CountAsync(
            u => u.Role == UserRole.Admin && u.IsActive,
            cancellationToken);
}
