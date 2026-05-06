using Microsoft.EntityFrameworkCore;
using WMS.Application.Catalog;
using WMS.Domain.Catalog;
using WMS.Infrastructure.Persistence;

namespace WMS.Infrastructure.Repositories;

public class CategoryRepository(AppDbContext db) : ICategoryRepository
{
    public Task<List<Category>> GetAllAsync(CancellationToken ct) =>
        db.Categories.OrderBy(c => c.Code).ToListAsync(ct);

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
        db.Categories.AnyAsync(c => c.Code == code.Trim().ToUpperInvariant(), ct);

    public void Add(Category category) => db.Categories.Add(category);
    public void Remove(Category category) => db.Categories.Remove(category);
    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
