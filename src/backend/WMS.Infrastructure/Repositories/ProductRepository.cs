using Microsoft.EntityFrameworkCore;
using WMS.Application.Catalog;
using WMS.Domain.Catalog;
using WMS.Infrastructure.Persistence;

namespace WMS.Infrastructure.Repositories;

public class ProductRepository(AppDbContext db) : IProductRepository
{
    public Task<List<Product>> GetAllAsync(ProductStatus? status, CancellationToken ct)
    {
        var query = db.Products.Include(p => p.Units).AsQueryable();
        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);
        return query.OrderBy(p => p.Code).ToListAsync(ct);
    }

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Products.Include(p => p.Units).FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        db.Products.AnyAsync(p => p.Code == code.Trim().ToUpperInvariant(), ct);

    public void Add(Product product) => db.Products.Add(product);
    public void Remove(Product product) => db.Products.Remove(product);
    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
