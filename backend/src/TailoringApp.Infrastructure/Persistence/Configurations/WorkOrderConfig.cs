using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TailoringApp.Domain.WorkOrders;

namespace TailoringApp.Infrastructure.Persistence.Configurations;

public sealed class WorkOrderConfig : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> b)
    {
        b.ToTable("WorkOrders");
        b.HasKey(x => x.Id);
        b.Property(x => x.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
        b.Property(x => x.Enabled).HasDefaultValue(true);
        b.Property(x => x.Status).HasConversion<int>();
        b.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        b.HasIndex(x => new { x.CustomerId, x.CreatedDate });

        b.OwnsMany(x => x.Items, i =>
        {
            i.ToTable("WorkOrderItems");
            i.WithOwner().HasForeignKey("WorkOrderId");
            i.HasKey("WorkOrderId", "Description");
            i.Property(p => p.Description).HasMaxLength(256);
            i.Property(p => p.Quantity).IsRequired();
            i.OwnsOne(p => p.UnitPrice, m =>
            {
                m.Property(pp => pp.Amount).HasColumnType("decimal(18,2)").HasColumnName("UnitPriceAmount");
                m.Property(pp => pp.Currency).HasMaxLength(3).HasColumnName("UnitPriceCurrency");
            });
        });

        b.OwnsOne(x => x.Discount, d =>
        {
            d.Property(p => p.Amount).HasColumnType("decimal(18,2)").HasColumnName("DiscountAmount");
            d.Property(p => p.Currency).HasMaxLength(3).HasColumnName("DiscountCurrency");
        });
    }
}
