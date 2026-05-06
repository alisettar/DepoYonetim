using WMS.Domain.Inventory;
using WMS.Domain.Inventory.Enums;

namespace WMS.Application.Inventory;

public interface IStockMovementRepository
{
    Task<List<StockMovement>> GetAllAsync(MovementType? type, Guid? productId, Guid? warehouseId, CancellationToken ct);
    Task<StockMovement?> GetByIdAsync(Guid id, CancellationToken ct);
    void Add(StockMovement movement);
    Task SaveAsync(CancellationToken ct);
}
