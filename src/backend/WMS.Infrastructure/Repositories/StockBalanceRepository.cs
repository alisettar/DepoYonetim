using Microsoft.EntityFrameworkCore;
using WMS.Application.Inventory;
using WMS.Domain.Inventory;
using WMS.Infrastructure.Persistence;

namespace WMS.Infrastructure.Repositories;

public class StockBalanceRepository(AppDbContext db) : IStockBalanceRepository
{
    public Task<StockBalance?> FindAsync(
        Guid productId,
        Guid? lotId,
        Guid? warehouseId,
        Guid? machineWarehouseId,
        CancellationToken ct)
        => db.StockBalances.FirstOrDefaultAsync(b =>
            b.ProductId == productId &&
            b.LotId == lotId &&
            b.WarehouseId == warehouseId &&
            b.MachineWarehouseId == machineWarehouseId, ct);

    public async Task<List<StockBalance>> GetAllAsync(Guid? productId, Guid? warehouseId, CancellationToken ct)
    {
        var query = db.StockBalances.AsQueryable();

        if (productId.HasValue)
            query = query.Where(b => b.ProductId == productId.Value);

        if (warehouseId.HasValue)
            query = query.Where(b => b.WarehouseId == warehouseId.Value);

        return await query.ToListAsync(ct);
    }

    public void Add(StockBalance balance) => db.StockBalances.Add(balance);

    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
