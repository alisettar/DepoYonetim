using WMS.Domain.Warehousing;

namespace WMS.Application.Warehousing;

public interface IWarehouseRepository
{
    Task<List<Warehouse>> GetAllAsync(WarehouseType? type, CancellationToken ct);
    Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    void Add(Warehouse warehouse);
    void Remove(Warehouse warehouse);
    Task SaveAsync(CancellationToken ct);
}
