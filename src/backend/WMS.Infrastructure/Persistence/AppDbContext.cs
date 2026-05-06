using Microsoft.EntityFrameworkCore;
using WMS.Shared.Common;

namespace WMS.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;
    private readonly string _tenantConnectionString;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ITenantContext tenantContext,
        string tenantConnectionString)
        : base(options)
    {
        _tenantContext = tenantContext;
        _tenantConnectionString = tenantConnectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseNpgsql(_tenantConnectionString);

        base.OnConfiguring(optionsBuilder);
    }

    public Guid TenantId => _tenantContext.TenantId;
    public Guid UserId => _tenantContext.UserId;

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
