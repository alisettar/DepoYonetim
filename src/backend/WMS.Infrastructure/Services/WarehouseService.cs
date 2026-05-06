using Microsoft.EntityFrameworkCore;
using WMS.Infrastructure.Persistence;
using WMS.Domain.Warehousing;

namespace WMS.Infrastructure.Services;

/// <summary>
/// Prototip Warehouse servisi — auth/tenant olmadan CRUD prototipleme.
/// </summary>
public class WarehouseService(PrototypeDbContext db)
{
    public Task<List<Warehouse>> GetAllAsync()
        => db.Warehouses.Include(w => w.Locations).ToListAsync();

    public Task<Warehouse?> GetByIdAsync(Guid id)
        => db.Warehouses.Include(w => w.Locations).FirstOrDefaultAsync(w => w.Id == id);

    public Task<Warehouse> CreateAsync(string code, string name, string? address = null)
    {
        var warehouse = Warehouse.Create(Guid.NewGuid(), code, name, Guid.Empty, address);
        db.Warehouses.Add(warehouse);
        return Task.FromResult(warehouse);
    }

    public async Task<(bool Success, Warehouse? Warehouse)> UpdateAsync(Guid id, string name, string? address = null)
    {
        var warehouse = await db.Warehouses.FindAsync(id)
            ?? throw new InvalidOperationException($"Warehouse {id} not found.");

        warehouse.Update(name, address);
        return (true, warehouse);
    }

    public async Task DeleteAsync(Guid id)
    {
        var warehouse = await db.Warehouses.FindAsync(id);
        if (warehouse != null)
            db.Warehouses.Remove(warehouse);
    }

    public async Task<int> SaveAsync() => await db.SaveChangesAsync();
}
