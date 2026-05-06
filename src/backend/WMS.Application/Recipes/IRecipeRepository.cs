using WMS.Domain.Recipes;

namespace WMS.Application.Recipes;

public interface IRecipeRepository
{
    Task<List<Recipe>> GetAllAsync(CancellationToken ct);
    Task<Recipe?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Recipe?> GetByIdWithVersionsAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsForProductAsync(Guid productId, CancellationToken ct);
    void Add(Recipe recipe);
    void Remove(Recipe recipe);
    Task SaveAsync(CancellationToken ct);
}
