using WMS.Domain.Inventory;

namespace WMS.Application.Inventory;

public interface IFifoLayerRepository
{
    Task<List<FifoLayer>> GetAvailableLayersAsync(Guid productId, Guid? warehouseId, Guid? machineWarehouseId, CancellationToken ct);
    Task<List<FifoLayer>> GetLayersByLotIdAsync(Guid lotId, CancellationToken ct);
    void Add(FifoLayer layer);
    Task SaveAsync(CancellationToken ct);
}
