using Microsoft.EntityFrameworkCore;
using WMS.Application.Warehousing;
using WMS.Domain.Warehousing;
using WMS.Infrastructure.Persistence;

namespace WMS.Infrastructure.Repositories;

public class WarehouseRepository(AppDbContext db) : IWarehouseRepository
{
    public Task<List<Warehouse>> GetAllAsync(WarehouseType? type, CancellationToken ct)
    {
        var query = db.Warehouses.Include(w => w.Locations).AsQueryable();
        if (type.HasValue)
            query = query.Where(w => w.Type == type.Value);
        return query.ToListAsync(ct);
    }

    public Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken ct)
        => db.Warehouses.Include(w => w.Locations).FirstOrDefaultAsync(w => w.Id == id, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct)
        => db.Warehouses.AnyAsync(w => w.Code == code.ToUpperInvariant(), ct);

    public void Add(Warehouse warehouse) => db.Warehouses.Add(warehouse);

    public void Remove(Warehouse warehouse) => db.Warehouses.Remove(warehouse);

    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
