using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Inventory;

namespace WMS.Infrastructure.Persistence.Configurations;

public class LotConfiguration : IEntityTypeConfiguration<Lot>
{
    public void Configure(EntityTypeBuilder<Lot> builder)
    {
        builder.ToTable("lots");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.LotNumber).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Source)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(e => e.QualityStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.HasIndex(e => new { e.ProductId, e.LotNumber }).IsUnique();
    }
}
