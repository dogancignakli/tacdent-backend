using Microsoft.EntityFrameworkCore;
using Tacdent.Core.Entities;

namespace Tacdent.Data.Context;

public class TacdentDbContext(DbContextOptions<TacdentDbContext> options) : DbContext(options)
{
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Consent> Consents => Set<Consent>();
    public DbSet<DentalService> Services => Set<DentalService>();
    public DbSet<Testimonial> Testimonials => Set<Testimonial>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TacdentDbContext).Assembly);
    }
}
