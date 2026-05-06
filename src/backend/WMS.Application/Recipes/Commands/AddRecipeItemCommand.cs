using MediatR;
using WMS.Application.Recipes.Dtos;
using WMS.Domain.Recipes;
using WMS.Shared.Exceptions;
using WMS.Shared.Result;

namespace WMS.Application.Recipes.Commands;

public record AddRecipeItemCommand(
    Guid RecipeId,
    Guid VersionId,
    Guid ProductId,
    decimal Quantity,
    Guid UnitId,
    decimal? WastePercent = null,
    decimal? WasteFixed = null,
    int SortOrder = 0) : IRequest<Result<RecipeItemDto>>;

public class AddRecipeItemHandler(IRecipeRepository repo)
    : IRequestHandler<AddRecipeItemCommand, Result<RecipeItemDto>>
{
    public async Task<Result<RecipeItemDto>> Handle(AddRecipeItemCommand request, CancellationToken ct)
    {
        var recipe = await repo.GetByIdWithVersionsAsync(request.RecipeId, ct);
        if (recipe is null)
            return Result.Failure<RecipeItemDto>("NOT_FOUND", "Reçete bulunamadı.");

        try
        {
            var version = recipe.GetVersion(request.VersionId);
            var item = RecipeItem.Create(
                request.VersionId,
                request.ProductId,
                request.Quantity,
                request.UnitId,
                request.WastePercent,
                request.WasteFixed,
                request.SortOrder);
            await repo.AddRecipeItemAsync(item, ct);
            await repo.SaveAsync(ct);
            return Result.Success(RecipeItemDto.FromEntity(item));
        }
        catch (BusinessException ex)
        {
            return Result.Failure<RecipeItemDto>(ex.ErrorCode, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<RecipeItemDto>("VALIDATION_ERROR", ex.Message);
        }
    }
}
