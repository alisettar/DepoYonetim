using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Domain.Recipes;

namespace WMS.Infrastructure.Persistence.Configurations;

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.ToTable("recipes");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.CreatedAt).IsRequired().ValueGeneratedOnAdd();

        builder.HasMany(e => e.Versions)
            .WithOne()
            .HasForeignKey(e => e.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RecipeVersionConfiguration : IEntityTypeConfiguration<RecipeVersion>
{
    public void Configure(EntityTypeBuilder<RecipeVersion> builder)
    {
        builder.ToTable("recipe_versions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.OutputQuantity).IsRequired().HasPrecision(18, 4);
        builder.Property(e => e.ValidFrom).IsRequired();
        builder.HasIndex(e => new { e.RecipeId, e.VersionNo }).IsUnique();

        builder.HasMany(e => e.Items)
            .WithOne()
            .HasForeignKey(e => e.RecipeVersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RecipeItemConfiguration : IEntityTypeConfiguration<RecipeItem>
{
    public void Configure(EntityTypeBuilder<RecipeItem> builder)
    {
        builder.ToTable("recipe_items");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Quantity).IsRequired().HasPrecision(18, 4);
        builder.Property(e => e.WastePercent).HasPrecision(5, 2);
        builder.Property(e => e.WasteFixed).HasPrecision(18, 4);

        builder.HasMany(e => e.Alternatives)
            .WithOne()
            .HasForeignKey(e => e.RecipeItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AlternativeMaterialConfiguration : IEntityTypeConfiguration<AlternativeMaterial>
{
    public void Configure(EntityTypeBuilder<AlternativeMaterial> builder)
    {
        builder.ToTable("recipe_item_alternatives");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Quantity).IsRequired().HasPrecision(18, 4);
        builder.HasIndex(e => new { e.RecipeItemId, e.ProductId }).IsUnique();
    }
}
