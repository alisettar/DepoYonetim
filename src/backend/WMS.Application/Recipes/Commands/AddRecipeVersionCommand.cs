using MediatR;
using WMS.Application.Recipes.Dtos;
using WMS.Domain.Recipes;
using WMS.Shared.Exceptions;
using WMS.Shared.Result;

namespace WMS.Application.Recipes.Commands;

public record AddRecipeVersionCommand(
    Guid RecipeId,
    DateTime ValidFrom,
    DateTime? ValidUntil,
    decimal OutputQuantity,
    Guid OutputUnitId) : IRequest<Result<RecipeVersionDto>>;

public class AddRecipeVersionHandler(IRecipeRepository repo)
    : IRequestHandler<AddRecipeVersionCommand, Result<RecipeVersionDto>>
{
    public async Task<Result<RecipeVersionDto>> Handle(AddRecipeVersionCommand request, CancellationToken ct)
    {
        var recipe = await repo.GetByIdWithVersionsAsync(request.RecipeId, ct);
        if (recipe is null)
            return Result.Failure<RecipeVersionDto>("NOT_FOUND", "Reçete bulunamadı.");

        try
        {
            var nextVersionNo = recipe.Versions.Count == 0
                ? 1
                : (int)recipe.Versions.Max(v => v.VersionNo) + 1;

            var version = RecipeVersion.Create(
                request.RecipeId,
                nextVersionNo,
                request.ValidFrom, request.ValidUntil,
                request.OutputQuantity, request.OutputUnitId);

            await repo.AddVersionAsync(version, ct);
            recipe.SetActiveStatus();
            await repo.SaveAsync(ct);
            return Result.Success(RecipeVersionDto.FromEntity(version));
        }
        catch (BusinessException ex)
        {
            return Result.Failure<RecipeVersionDto>(ex.ErrorCode, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<RecipeVersionDto>("VALIDATION_ERROR", ex.Message);
        }
    }
}
