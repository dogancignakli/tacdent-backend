using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tacdent.Core.Entities;

namespace Tacdent.Data.Configurations;

public class DentalServiceConfiguration : IEntityTypeConfiguration<DentalService>
{
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<DentalService> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.NameTr).IsRequired().HasMaxLength(120);
        builder.Property(s => s.NameEn).IsRequired().HasMaxLength(120);
        builder.Property(s => s.DescriptionTr).IsRequired().HasMaxLength(500);
        builder.Property(s => s.DescriptionEn).IsRequired().HasMaxLength(500);
        builder.Property(s => s.Icon).HasMaxLength(50);
        builder.Property(s => s.PriceFromTry).HasPrecision(10, 2);
        builder.Property(s => s.PriceFromEur).HasPrecision(10, 2);

        builder.HasData(
            Service(
                1,
                "Genel Muayene",
                "General Checkup",
                "Kapsamlı ağız muayenesi, temizlik ve koruyucu bakım.",
                "Comprehensive oral exam, cleaning, and preventive care.",
                "checkup",
                75m,
                25m,
                45,
                1),
            Service(
                2,
                "Diş Beyazlatma",
                "Teeth Whitening",
                "Daha parlak ve özgüvenli bir gülüş için profesyonel beyazlatma.",
                "Professional whitening for a brighter, confident smile.",
                "whitening",
                199m,
                65m,
                60,
                2),
            Service(
                3,
                "Diş İmplantı",
                "Dental Implants",
                "Doğal görünümlü kalıcı diş değişimi.",
                "Permanent tooth replacement with natural-looking results.",
                "implant",
                1200m,
                350m,
                90,
                3),
            Service(
                4,
                "Ortodonti",
                "Orthodontics",
                "Şeffaf plaklar ve teller ile düzgün dişler.",
                "Clear aligners and braces for straighter teeth.",
                "ortho",
                2500m,
                800m,
                60,
                4),
            Service(
                5,
                "Acil Bakım",
                "Emergency Care",
                "Ağrı, kırık ve acil durumlar için aynı gün müdahale.",
                "Same-day relief for pain, breaks, and urgent issues.",
                "emergency",
                120m,
                40m,
                30,
                5)
        );
    }

    private static DentalService Service(
        int id,
        string nameTr,
        string nameEn,
        string descriptionTr,
        string descriptionEn,
        string icon,
        decimal priceFromTry,
        decimal priceFromEur,
        int durationMinutes,
        int displayOrder) =>
        new()
        {
            Id = id,
            NameTr = nameTr,
            NameEn = nameEn,
            DescriptionTr = descriptionTr,
            DescriptionEn = descriptionEn,
            Icon = icon,
            PriceFromTry = priceFromTry,
            PriceFromEur = priceFromEur,
            DurationMinutes = durationMinutes,
            DisplayOrder = displayOrder,
            IsActive = true,
            CreatedAt = SeedTimestamp,
            UpdatedAt = SeedTimestamp,
        };
}
