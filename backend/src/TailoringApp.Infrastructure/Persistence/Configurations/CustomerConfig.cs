using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TailoringApp.Domain.Customers;
using TailoringApp.Domain.Customers.Entities;
using TailoringApp.Domain.Customers.ValueObjects;

namespace TailoringApp.Infrastructure.Persistence.Configurations;

public class CustomerConfig : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);
    builder.HasQueryFilter(c => c.Enabled);

        builder.Property(c => c.CustomerNumber)
            .HasMaxLength(32)
            .IsRequired();

        builder.HasIndex(c => c.CustomerNumber).IsUnique();

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(c => c.TotalSpent)
            .HasColumnType("decimal(18,2)");

        builder.Property(c => c.RegistrationDate)
            .IsRequired();

        builder.Property(c => c.CreatedDate)
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();

        builder.Property(c => c.Enabled)
            .HasDefaultValue(true)
            .IsRequired();

        builder.OwnsOne(c => c.PersonalInfo, pi =>
        {
            pi.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
            pi.Property(p => p.LastName).HasMaxLength(100).IsRequired();
            pi.Property(p => p.DateOfBirth);
        });

        builder.OwnsOne(c => c.ContactInfo, ci =>
        {
            ci.Property(p => p.Email).HasMaxLength(256).IsRequired();
            ci.Property(p => p.Phone).HasMaxLength(32);
            ci.Property(p => p.Address).HasMaxLength(256);
            ci.HasIndex(p => p.Email).IsUnique();
        });

        builder.OwnsOne(c => c.Preferences, pref =>
        {
            pref.Property(p => p.Style).HasMaxLength(64);
            pref.Property(p => p.Fit).HasMaxLength(64);
            pref.Property(p => p.Notes).HasMaxLength(64);
        });

        builder.Navigation(c => c.MeasurementHistory)
            .HasField("_measurements")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(c => c.Notes)
            .HasField("_notes")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(c => c.MeasurementHistory, m =>
        {
            m.ToTable("Measurements");
            m.WithOwner().HasForeignKey("CustomerId");
            m.HasKey(x => x.Id);
            m.Property(x => x.Id).ValueGeneratedNever();
            m.Property(x => x.Date).IsRequired();
            m.Property(x => x.Chest).HasColumnType("decimal(18,2)");
            m.Property(x => x.Waist).HasColumnType("decimal(18,2)");
            m.Property(x => x.Hips).HasColumnType("decimal(18,2)");
            m.Property(x => x.Sleeve).HasColumnType("decimal(18,2)");
        });

        builder.OwnsMany(c => c.Notes, n =>
        {
            n.ToTable("CustomerNotes");
            n.WithOwner().HasForeignKey("CustomerId");
            n.HasKey(x => x.Id);
            n.Property(x => x.Id).ValueGeneratedNever();
            n.Property(x => x.Date).IsRequired();
            n.Property(x => x.Text).HasMaxLength(1000).IsRequired();
            n.Property(x => x.Author).HasMaxLength(100);
        });
    }
}
