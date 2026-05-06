using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Inventory;

namespace WMS.Infrastructure.Persistence.Configurations;

public class StockBalanceConfiguration : IEntityTypeConfiguration<StockBalance>
{
    public void Configure(EntityTypeBuilder<StockBalance> builder)
    {
        builder.ToTable("stock_balances");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Quantity)
            .HasPrecision(18, 4);
        builder.HasIndex(e => new { e.ProductId, e.LotId, e.WarehouseId, e.MachineWarehouseId })
            .IsUnique();
    }
}
