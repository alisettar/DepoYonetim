using Microsoft.EntityFrameworkCore;
using WMS.Application.Inventory;
using WMS.Domain.Inventory;
using WMS.Infrastructure.Persistence;

namespace WMS.Infrastructure.Repositories;

public class FifoLayerRepository(AppDbContext db) : IFifoLayerRepository
{
    public Task<List<FifoLayer>> GetAvailableLayersAsync(
        Guid productId,
        Guid? warehouseId,
        Guid? machineWarehouseId,
        CancellationToken ct)
        => db.FifoLayers
            .Where(l =>
                l.ProductId == productId &&
                l.WarehouseId == warehouseId &&
                l.MachineWarehouseId == machineWarehouseId &&
                !l.IsClosed)
            .OrderBy(l => l.ReceiptDate)
            .ThenBy(l => l.Id)
            .ToListAsync(ct);

    public void Add(FifoLayer layer) => db.FifoLayers.Add(layer);

    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
