using Microsoft.EntityFrameworkCore;
using Tacdent.Core.Entities;

namespace Tacdent.Data.Context;

public class TacdentDbContext(DbContextOptions<TacdentDbContext> options) : DbContext(options)
{
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<DentalService> Services => Set<DentalService>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TacdentDbContext).Assembly);
    }
}
