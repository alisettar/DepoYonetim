using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Warehousing;

namespace WMS.Infrastructure.Persistence.Configurations;

public class WarehouseLocationConfiguration : IEntityTypeConfiguration<WarehouseLocation>
{
    public void Configure(EntityTypeBuilder<WarehouseLocation> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.WarehouseId).IsRequired();
        builder.Property(e => e.Zone).IsRequired().HasMaxLength(10);
        builder.Property(e => e.Aisle).IsRequired().HasMaxLength(10);
        builder.Property(e => e.Section).IsRequired().HasMaxLength(10);
        builder.Property(e => e.Bin).IsRequired().HasMaxLength(10);
        builder.Property(e => e.IsActive).IsRequired();

        builder.HasIndex(e => new { e.WarehouseId, e.Zone, e.Aisle, e.Section, e.Bin }).IsUnique();
    }
}
