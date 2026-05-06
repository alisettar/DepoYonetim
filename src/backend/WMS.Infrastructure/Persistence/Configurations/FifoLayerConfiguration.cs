using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Inventory;

namespace WMS.Infrastructure.Persistence.Configurations;

public class FifoLayerConfiguration : IEntityTypeConfiguration<FifoLayer>
{
    public void Configure(EntityTypeBuilder<FifoLayer> builder)
    {
        builder.ToTable("fifo_layers");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RemainingQuantity)
            .HasPrecision(18, 4);
        builder.Property(e => e.UnitCost)
            .HasPrecision(18, 4);
        builder.HasIndex(e => new { e.ProductId, e.WarehouseId, e.IsClosed });
    }
}
