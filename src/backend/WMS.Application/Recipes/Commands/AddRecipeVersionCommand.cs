using MediatR;
using WMS.Application.Recipes.Dtos;
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
            var version = recipe.AddVersion(
                request.ValidFrom, request.ValidUntil,
                request.OutputQuantity, request.OutputUnitId);
            await repo.SaveAsync(ct);
            return Result.Success(RecipeVersionDto.FromEntity(version));
        }
        catch (BusinessException ex)
        {
            return Result.Failure<RecipeVersionDto>(ex.Code, ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<RecipeVersionDto>("VALIDATION_ERROR", ex.Message);
        }
    }
}
