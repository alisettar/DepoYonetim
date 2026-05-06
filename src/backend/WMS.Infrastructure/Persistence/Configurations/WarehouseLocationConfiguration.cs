using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Warehousing;

namespace WMS.Infrastructure.Persistence.Configurations;

public class WarehouseLocationConfiguration : IEntityTypeConfiguration<WarehouseLocation>
{
    public void Configure(EntityTypeBuilder<WarehouseLocation> builder)
    {
        builder.ToTable("warehouse_locations");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.WarehouseId).IsRequired();
        builder.Property(e => e.Code).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Capacity).HasPrecision(18, 4);
        builder.Property(e => e.IsActive).IsRequired();

        builder.HasIndex(e => new { e.WarehouseId, e.Code }).IsUnique();
    }
}
