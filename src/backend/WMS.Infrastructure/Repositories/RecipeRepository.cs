using Microsoft.EntityFrameworkCore;
using WMS.Application.Recipes;
using WMS.Domain.Recipes;
using WMS.Infrastructure.Persistence;

namespace WMS.Infrastructure.Repositories;

public class RecipeRepository(AppDbContext db) : IRecipeRepository
{
    public Task<List<Recipe>> GetAllAsync(CancellationToken ct) =>
        db.Recipes
            .Include(r => r.Versions)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);

    public Task<Recipe?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Recipes.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<Recipe?> GetByIdWithVersionsAsync(Guid id, CancellationToken ct) =>
        db.Recipes
            .Include(r => r.Versions)
                .ThenInclude(v => v.Items)
                    .ThenInclude(i => i.Alternatives)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<bool> ExistsForProductAsync(Guid productId, CancellationToken ct) =>
        db.Recipes.AnyAsync(r => r.ProductId == productId, ct);

    public void Add(Recipe recipe) => db.Recipes.Add(recipe);
    public void Remove(Recipe recipe) => db.Recipes.Remove(recipe);
    public Task SaveAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
