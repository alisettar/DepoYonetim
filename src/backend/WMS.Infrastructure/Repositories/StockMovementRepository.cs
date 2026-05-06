using Microsoft.EntityFrameworkCore;
using WMS.Application.Inventory;
using WMS.Domain.Inventory;
using WMS.Domain.Inventory.Enums;
using WMS.Infrastructure.Persistence;

namespace WMS.Infrastructure.Repositories;

public class StockMovementRepository(AppDbContext db) : IStockMovementRepository
{
    public async Task<List<StockMovement>> GetAllAsync(
        MovementType? type,
        Guid? productId,
        Guid? warehouseId,
        CancellationToken ct)
    {
        var query = db.StockMovements.AsQueryable();

        if (type.HasValue)
            query = query.Where(m => m.Type == type.Value);

        if (productId.HasValue)
            query = query.Where(m => m.ProductId == productId.Value);

        if (warehouseId.HasValue)
            query = query.Where(m => m.WarehouseId == warehouseId.Value);

        return await query.OrderByDescending(m => m.OccurredAt).ToListAsync(ct);
    }

    public Task<StockMovement?> GetByIdAsync(Guid id, CancellationToken ct)
        => db.StockMovements.FirstOrDefaultAsync(m => m.Id == id, ct);

    public void Add(StockMovement movement) => db.StockMovements.Add(movement);

    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
