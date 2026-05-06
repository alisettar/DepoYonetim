using WMS.Domain.Inventory;

namespace WMS.Application.Inventory;

public interface ILotRepository
{
    Task<Lot?> FindByNumberAsync(Guid productId, string lotNumber, CancellationToken ct);
    Task<List<Lot>> GetAllAsync(CancellationToken ct);
    Task<Lot?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<(List<Lot> Items, int TotalCount)> GetAllPagedAsync(
        Guid? productId, string? lotNumberFilter, string? qualityStatus,
        int page, int pageSize, CancellationToken ct);
    void Add(Lot lot);
    Task SaveAsync(CancellationToken ct);
}
