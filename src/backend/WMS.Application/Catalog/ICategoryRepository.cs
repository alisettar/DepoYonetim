using WMS.Domain.Catalog;

namespace WMS.Application.Catalog;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(CancellationToken ct);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    void Add(Category category);
    void Remove(Category category);
    Task SaveAsync(CancellationToken ct);
}
