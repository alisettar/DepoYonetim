using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Inventory;

namespace WMS.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movements");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(e => e.Quantity)
            .HasPrecision(18, 4);
        builder.Property(e => e.UnitCost)
            .HasPrecision(18, 4);
        builder.Property(e => e.ReferenceType)
            .HasMaxLength(100);
        builder.Property(e => e.VoidReason)
            .HasMaxLength(500);
        builder.Property(e => e.Note)
            .HasMaxLength(500);
        builder.HasIndex(e => new { e.ProductId, e.OccurredAt })
            .HasDatabaseName("IX_stock_movements_product_date");
    }
}
