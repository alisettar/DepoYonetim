using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Warehousing;

namespace WMS.Infrastructure.Persistence.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.TenantId).IsRequired();
        builder.Property(e => e.IsActive).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => new { e.TenantId, e.Code }).IsUnique();

        builder.Property(e => e.Address).HasMaxLength(500);

        builder.HasMany(e => e.Locations)
            .WithOne()
            .HasForeignKey(e => e.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
