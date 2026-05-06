using Microsoft.EntityFrameworkCore;
using WMS.Domain.Identity;
using WMS.Domain.Warehousing;
using WMS.Infrastructure.Persistence.Configurations;
using WMS.Infrastructure.Services;
using WMS.Shared.Common;

namespace WMS.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    // Runtime constructor — DI ile çalışır, tenant connection'ı çözer
    public AppDbContext(ITenantContext tenantContext, ICachedTenantConnectionFactory connectionFactory)
        : base(BuildOptions(
            connectionFactory.GetConnectionString(tenantContext.TenantId)
            ?? throw new InvalidOperationException($"Tenant {tenantContext.TenantId} için bağlantı bulunamadı.")))
    {
    }

    // Dev init / tooling constructor
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    private static DbContextOptions<AppDbContext> BuildOptions(string connectionString)
        => new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connectionString).Options;

    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();
    public DbSet<SuperAdmin> SuperAdmins => Set<SuperAdmin>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<WarehouseLocation> WarehouseLocations => Set<WarehouseLocation>();
    public DbSet<MachineWarehouse> MachineWarehouses => Set<MachineWarehouse>();

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

        base.OnModelCreating(modelBuilder);
    }
}
