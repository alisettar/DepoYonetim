using WMS.Domain.Inventory;

namespace WMS.Application.Inventory;

public interface ILotRepository
{
    Task<Lot?> FindByNumberAsync(Guid productId, string lotNumber, CancellationToken ct);
    Task<List<Lot>> GetAllAsync(CancellationToken ct);
    void Add(Lot lot);
    Task SaveAsync(CancellationToken ct);
}
