using WMS.Domain.Inventory;

namespace WMS.Application.Inventory;

public interface IStockBalanceRepository
{
    Task<StockBalance?> FindAsync(Guid productId, Guid? lotId, Guid? warehouseId, Guid? machineWarehouseId, CancellationToken ct);
    Task<List<StockBalance>> GetAllAsync(Guid? productId, Guid? warehouseId, CancellationToken ct);
    void Add(StockBalance balance);
    Task SaveAsync(CancellationToken ct);
}
