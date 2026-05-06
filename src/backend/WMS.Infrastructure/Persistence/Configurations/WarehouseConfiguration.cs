using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Warehousing;

namespace WMS.Infrastructure.Persistence.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("warehouses");
        builder.HasKey(e => e.Id);

        builder.HasDiscriminator<string>("warehouse_kind")
            .HasValue<Warehouse>("standard")
            .HasValue<MachineWarehouse>("machine");

        builder.Property(e => e.Code).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Type).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.CreatedAt).IsRequired();

        builder.HasIndex(e => e.Code).IsUnique();

        builder.HasMany(e => e.Locations)
            .WithOne()
            .HasForeignKey(e => e.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MachineWarehouseConfiguration : IEntityTypeConfiguration<MachineWarehouse>
{
    public void Configure(EntityTypeBuilder<MachineWarehouse> builder)
    {
        builder.Property(e => e.MachineCode).IsRequired().HasMaxLength(50);
        builder.Property(e => e.MachineStatus).IsRequired().HasConversion<string>().HasMaxLength(20);
    }
}
