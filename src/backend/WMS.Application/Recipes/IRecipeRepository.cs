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
    Task<RecipeVersion> AddVersionAsync(RecipeVersion version, CancellationToken ct);
    Task<RecipeItem> AddRecipeItemAsync(RecipeItem item, CancellationToken ct);
    Task<AlternativeMaterial> AddAlternativeAsync(AlternativeMaterial alt, CancellationToken ct);
}
