using MediatR;
using WMS.Application.Recipes.Dtos;
using WMS.Shared.Exceptions;
using WMS.Shared.Result;

namespace WMS.Application.Recipes.Commands;

public record UpdateRecipeVersionCommand(
    Guid RecipeId,
    Guid VersionId,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    decimal? OutputQuantity,
    Guid? OutputUnitId) : IRequest<Result<RecipeVersionDto>>;

public class UpdateRecipeVersionHandler(IRecipeRepository repo)
    : IRequestHandler<UpdateRecipeVersionCommand, Result<RecipeVersionDto>>
{
    public async Task<Result<RecipeVersionDto>> Handle(UpdateRecipeVersionCommand request, CancellationToken ct)
    {
        var recipe = await repo.GetByIdWithVersionsAsync(request.RecipeId, ct);
        if (recipe is null)
            return Result.Failure<RecipeVersionDto>("NOT_FOUND", "Reçete bulunamadı.");

        try
        {
            var version = recipe.GetVersion(request.VersionId);
            version.Update(
                request.ValidFrom ?? version.ValidFrom,
                request.ValidUntil,
                request.OutputQuantity ?? version.OutputQuantity,
                request.OutputUnitId ?? version.OutputUnitId);
            await repo.SaveAsync(ct);
            return Result.Success(RecipeVersionDto.FromEntity(version));
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<RecipeVersionDto>("VALIDATION_ERROR", ex.Message);
        }
    }
}
