using WMS.Domain.Recipes;

namespace WMS.Application.Recipes.Dtos;

public record AlternativeMaterialDto(
    Guid Id,
    Guid ProductId,
    int Priority,
    decimal Quantity,
    Guid UnitId)
{
    public static AlternativeMaterialDto FromEntity(AlternativeMaterial e) =>
        new(e.Id, e.ProductId, e.Priority, e.Quantity, e.UnitId);
}

public record RecipeItemDto(
    Guid Id,
    Guid ProductId,
    decimal Quantity,
    Guid UnitId,
    decimal? WastePercent,
    decimal? WasteFixed,
    int SortOrder,
    List<AlternativeMaterialDto> Alternatives)
{
    public static RecipeItemDto FromEntity(RecipeItem e) =>
        new(e.Id, e.ProductId, e.Quantity, e.UnitId,
            e.WastePercent, e.WasteFixed, e.SortOrder,
            e.Alternatives.Select(AlternativeMaterialDto.FromEntity).ToList());
}

public record RecipeVersionDto(
    Guid Id,
    int VersionNo,
    DateTime ValidFrom,
    DateTime? ValidUntil,
    bool IsActive,
    decimal OutputQuantity,
    Guid OutputUnitId,
    List<RecipeItemDto> Items)
{
    public static RecipeVersionDto FromEntity(RecipeVersion e) =>
        new(e.Id, e.VersionNo, e.ValidFrom, e.ValidUntil,
            e.IsActive, e.OutputQuantity, e.OutputUnitId,
            e.Items.OrderBy(i => i.SortOrder).Select(RecipeItemDto.FromEntity).ToList());
}

public record RecipeDto(
    Guid Id,
    Guid ProductId,
    string Name,
    string Status,
    DateTime CreatedAt,
    List<RecipeVersionDto> Versions)
{
    public static RecipeDto FromEntity(Recipe e) =>
        new(e.Id, e.ProductId, e.Name, e.Status.ToString(),
            e.CreatedAt,
            e.Versions.OrderBy(v => v.VersionNo).Select(RecipeVersionDto.FromEntity).ToList());
}

public record RecipeSummaryDto(
    Guid Id,
    Guid ProductId,
    string Name,
    string Status,
    int VersionCount,
    DateTime CreatedAt)
{
    public static RecipeSummaryDto FromEntity(Recipe e) =>
        new(e.Id, e.ProductId, e.Name, e.Status.ToString(), e.Versions.Count, e.CreatedAt);
}

public record BomLineDto(
    Guid ProductId,
    int Level,
    decimal Quantity,
    Guid UnitId,
    decimal? WastePercent,
    decimal? WasteFixed,
    decimal EffectiveQuantity);
