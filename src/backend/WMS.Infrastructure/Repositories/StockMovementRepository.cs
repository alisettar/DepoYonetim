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

    public async Task<(List<StockMovement> Items, int TotalCount)> GetAllPagedForLotAsync(
        Guid lotId, int page, int pageSize, CancellationToken ct)
    {
        var query = db.StockMovements
            .Where(m => m.LotId == lotId && !m.IsVoided)
            .OrderByDescending(m => m.OccurredAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public Task<List<StockMovement>> GetTraceChainAsync(Guid lotId, CancellationToken ct)
        => db.StockMovements
            .Where(m => m.LotId == lotId && !m.IsVoided)
            .OrderByDescending(m => m.OccurredAt)
            .ToListAsync(ct);

    public void Add(StockMovement movement) => db.StockMovements.Add(movement);

    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
