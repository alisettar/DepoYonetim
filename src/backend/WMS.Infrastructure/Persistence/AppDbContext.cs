using Microsoft.EntityFrameworkCore;
using WMS.Shared.Common;

namespace WMS.Infrastructure.Persistence;

public class AppDbContext(
    DbContextOptions<AppDbContext> options,
    ITenantContext tenantContext,
    string tenantConnectionString) : DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseNpgsql(tenantConnectionString);

        base.OnConfiguring(optionsBuilder);
    }

    public Guid TenantId => tenantContext.TenantId;
    public Guid UserId => tenantContext.UserId;

    // Placeholder for tenant-specific entity sets — populated in M2
    // DbSet<Product> Products => Set<Product>();
    // DbSet<Warehouse> Warehouses => Set<Warehouse>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Tenant-specific entity sets will be added in M2
        // Placeholder: configure global query filters here

        base.OnModelCreating(modelBuilder);
    }
}
