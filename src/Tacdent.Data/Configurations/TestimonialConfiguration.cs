using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tacdent.Core.Entities;

namespace Tacdent.Data.Configurations;

public class TestimonialConfiguration : IEntityTypeConfiguration<Testimonial>
{
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<Testimonial> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.AuthorName).IsRequired().HasMaxLength(120);
        builder.Property(t => t.QuoteTr).IsRequired().HasMaxLength(1000);
        builder.Property(t => t.QuoteEn).HasMaxLength(1000);

        builder.HasData(
            new Testimonial
            {
                Id = 1,
                AuthorName = "Sarah M.",
                QuoteTr = "Kapıdan girdiğim andan itibaren kendimi rahat hissettim. Şimdiye kadarki en iyi diş deneyimim.",
                QuoteEn = "The team made me feel calm from the moment I walked in. Best dental experience I've had.",
                Rating = 5,
                IsActive = true,
                DisplayOrder = 1,
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp,
            },
            new Testimonial
            {
                Id = 2,
                AuthorName = "James L.",
                QuoteTr = "Çevrimiçi randevu almak kolaydı ve hatırlatma sayesinde randevumu unutmadım.",
                QuoteEn = "Booking online was easy and the reminder meant I never missed my appointment.",
                Rating = 5,
                IsActive = true,
                DisplayOrder = 2,
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp,
            },
            new Testimonial
            {
                Id = 3,
                AuthorName = "Ayşe K.",
                QuoteTr = "Profesyonel, nazik ve her adımı şeffaf anlattılar. Kesinlikle tavsiye ederim.",
                QuoteEn = "Professional, gentle, and transparent about every step. I highly recommend them.",
                Rating = 5,
                IsActive = true,
                DisplayOrder = 3,
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp,
            }
        );
    }
}
