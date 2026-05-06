using WMS.Domain.Catalog;

namespace WMS.Application.Catalog;

public interface IUnitRepository
{
    Task<List<Unit>> GetAllAsync(CancellationToken ct);
    Task<Unit?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    void Add(Unit unit);
    void Remove(Unit unit);
    Task SaveAsync(CancellationToken ct);
}
