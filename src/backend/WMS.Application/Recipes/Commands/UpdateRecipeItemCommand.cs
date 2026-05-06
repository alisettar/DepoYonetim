using MediatR;
using WMS.Application.Recipes.Dtos;
using WMS.Shared.Exceptions;
using WMS.Shared.Result;

namespace WMS.Application.Recipes.Commands;

public record UpdateRecipeItemCommand(
    Guid RecipeId,
    Guid VersionId,
    Guid ItemId,
    decimal Quantity,
    Guid UnitId,
    decimal? WastePercent,
    decimal? WasteFixed,
    int SortOrder) : IRequest<Result<RecipeItemDto>>;

public class UpdateRecipeItemHandler(IRecipeRepository repo)
    : IRequestHandler<UpdateRecipeItemCommand, Result<RecipeItemDto>>
{
    public async Task<Result<RecipeItemDto>> Handle(UpdateRecipeItemCommand request, CancellationToken ct)
    {
        try
        {
            var recipe = await repo.GetByIdWithVersionsAsync(request.RecipeId, ct)
                ?? throw new BusinessException("RECIPE_NOT_FOUND", "Reçete bulunamadı.");

            var version = recipe.GetVersion(request.VersionId);
            var item = version.GetItem(request.ItemId);

            item.Update(request.Quantity, request.UnitId, request.WastePercent, request.WasteFixed, request.SortOrder);

            await repo.SaveAsync(ct);
            return Result.Success(RecipeItemDto.FromEntity(item));
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<RecipeItemDto>("VALIDATION_ERROR", ex.Message);
        }
    }
}
