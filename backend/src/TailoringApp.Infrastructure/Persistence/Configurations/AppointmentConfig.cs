using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TailoringApp.Domain.Appointments;

namespace TailoringApp.Infrastructure.Persistence.Configurations;

public class AppointmentConfig : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> b)
    {
        b.ToTable("Appointments");
        b.HasKey(a => a.Id);
        b.Property(a => a.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
        b.Property(a => a.Enabled).HasDefaultValue(true);
        b.Property(a => a.Status).HasConversion<int>();

        b.Property(a => a.Notes).HasMaxLength(512);
        b.HasIndex(a => new { a.CustomerId, a.StartUtc, a.EndUtc });
    }
}
