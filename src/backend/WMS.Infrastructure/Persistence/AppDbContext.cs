using Microsoft.EntityFrameworkCore;
using WMS.Domain.Catalog;
using WMS.Domain.Identity;
using WMS.Domain.Inventory;
using WMS.Domain.Recipes;
using WMS.Domain.Warehousing;
using WMS.Infrastructure.Persistence.Configurations;
using WMS.Infrastructure.Services;
using WMS.Shared.Common;

namespace WMS.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(ITenantContext tenantContext, ICachedTenantConnectionFactory connectionFactory)
        : base(BuildOptions(
            connectionFactory.GetConnectionString(tenantContext.TenantId)
            ?? throw new InvalidOperationException($"Tenant {tenantContext.TenantId} için bağlantı bulunamadı.")))
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    private static DbContextOptions<AppDbContext> BuildOptions(string connectionString)
        => new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connectionString).Options;

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken ct = default)
    {
        NormalizeUtcTimestamps();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, ct);
    }

    public override int SaveChanges()
    {
        NormalizeUtcTimestamps();
        return base.SaveChanges();
    }

    private static DateTime NormalizeUtc(DateTime dt) =>
        dt.Kind != DateTimeKind.Utc ? DateTime.SpecifyKind(dt, DateTimeKind.Utc) : dt;

    private void NormalizeUtcTimestamps()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            foreach (var prop in entry.Properties)
            {
                if (prop.CurrentValue is DateTime dt)
                {
                    if (prop.Metadata.Name is "CreatedAt" or "UpdatedAt" or "OccurredAt" or "VoidedAt")
                    {
                        prop.CurrentValue = NormalizeUtc(dt);
                    }
                }
            }
        }
    }

    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();
    public DbSet<SuperAdmin> SuperAdmins => Set<SuperAdmin>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<WarehouseLocation> WarehouseLocations => Set<WarehouseLocation>();
    public DbSet<MachineWarehouse> MachineWarehouses => Set<MachineWarehouse>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductUnit> ProductUnits => Set<ProductUnit>();
    public DbSet<Lot> Lots => Set<Lot>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<StockBalance> StockBalances => Set<StockBalance>();
    public DbSet<FifoLayer> FifoLayers => Set<FifoLayer>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeVersion> RecipeVersions => Set<RecipeVersion>();
    public DbSet<RecipeItem> RecipeItems => Set<RecipeItem>();
    public DbSet<AlternativeMaterial> AlternativeMaterials => Set<AlternativeMaterial>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TenantUser>(entity =>
        {
            entity.ToTable("tenant_users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(256);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.RoleCode).IsRequired().HasMaxLength(64);
        });

        modelBuilder.Entity<SuperAdmin>(entity =>
        {
            entity.ToTable("super_admins");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(256);
        });

        modelBuilder.ApplyConfiguration(new WarehouseConfiguration());
        modelBuilder.ApplyConfiguration(new WarehouseLocationConfiguration());
        modelBuilder.ApplyConfiguration(new UnitConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new ProductUnitConfiguration());
        modelBuilder.ApplyConfiguration(new LotConfiguration());
        modelBuilder.ApplyConfiguration(new StockMovementConfiguration());
        modelBuilder.ApplyConfiguration(new StockBalanceConfiguration());
        modelBuilder.ApplyConfiguration(new FifoLayerConfiguration());
        modelBuilder.ApplyConfiguration(new RecipeConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
