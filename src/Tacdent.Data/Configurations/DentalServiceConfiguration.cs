using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tacdent.Core.Entities;

namespace Tacdent.Data.Configurations;

public class DentalServiceConfiguration : IEntityTypeConfiguration<DentalService>
{
    // Seed rows need deterministic timestamps; EF rejects non-constant values (e.g. DateTime.UtcNow) in HasData.
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<DentalService> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(120);
        builder.Property(s => s.Description).IsRequired().HasMaxLength(500);
        builder.Property(s => s.Icon).HasMaxLength(50);
        builder.Property(s => s.PriceFrom).HasPrecision(10, 2);

        builder.HasData(
            Service(1, "General Checkup", "Comprehensive oral exam, cleaning, and preventive care.", "checkup", 75m, 45),
            Service(2, "Teeth Whitening", "Professional whitening for a brighter, confident smile.", "whitening", 199m, 60),
            Service(3, "Dental Implants", "Permanent tooth replacement with natural-looking results.", "implant", 1200m, 90),
            Service(4, "Orthodontics", "Clear aligners and braces for straighter teeth.", "ortho", 2500m, 60),
            Service(5, "Emergency Care", "Same-day relief for pain, breaks, and urgent issues.", "emergency", 120m, 30)
        );
    }

    private static DentalService Service(
        int id,
        string name,
        string description,
        string icon,
        decimal priceFrom,
        int durationMinutes) =>
        new()
        {
            Id = id,
            Name = name,
            Description = description,
            Icon = icon,
            PriceFrom = priceFrom,
            DurationMinutes = durationMinutes,
            IsActive = true,
            CreatedAt = SeedTimestamp,
            UpdatedAt = SeedTimestamp
        };
}
