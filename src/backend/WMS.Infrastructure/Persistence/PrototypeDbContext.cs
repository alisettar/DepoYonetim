using Microsoft.EntityFrameworkCore;
using WMS.Domain.Warehousing;
using WMS.Infrastructure.Persistence.Configurations;

namespace WMS.Infrastructure.Persistence;

/// <summary>
/// Prototip DbContext — warehouse module için doğrudan bağlantı dizisi kullanır.
/// Multi-tenant modülüne geçildiğinde AppDbContext ile değiştirilecek.
/// </summary>
public class PrototypeDbContext : DbContext
{
    public PrototypeDbContext(string connectionString)
        : base(new DbContextOptionsBuilder<PrototypeDbContext>()
            .UseNpgsql(connectionString)
            .Options) { }

    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<WarehouseLocation> WarehouseLocations => Set<WarehouseLocation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new WarehouseConfiguration());
        modelBuilder.ApplyConfiguration(new WarehouseLocationConfiguration());
    }
}
