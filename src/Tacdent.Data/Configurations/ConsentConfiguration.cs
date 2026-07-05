using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tacdent.Core.Entities;

namespace Tacdent.Data.Configurations;

public class ConsentConfiguration : IEntityTypeConfiguration<Consent>
{
    public void Configure(EntityTypeBuilder<Consent> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.TextVersion).IsRequired().HasMaxLength(50);
        builder.Property(c => c.PatientName).IsRequired().HasMaxLength(120);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Phone).IsRequired().HasMaxLength(30);
        builder.Property(c => c.IpAddress).HasMaxLength(45);

        builder.Property(c => c.ConsentType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(c => c.AppointmentId);
        builder.HasIndex(c => new { c.ConsentType, c.TextVersion });

        builder.HasOne(c => c.Appointment)
            .WithMany(a => a.Consents)
            .HasForeignKey(c => c.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
