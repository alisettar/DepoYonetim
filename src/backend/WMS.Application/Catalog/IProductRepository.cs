using WMS.Domain.Catalog;

namespace WMS.Application.Catalog;

public interface IProductRepository
{
    Task<List<Product>> GetAllAsync(ProductStatus? status, CancellationToken ct);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    void Add(Product product);
    void Remove(Product product);
    Task SaveAsync(CancellationToken ct);
}
