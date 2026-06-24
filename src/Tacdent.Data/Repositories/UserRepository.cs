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
}
