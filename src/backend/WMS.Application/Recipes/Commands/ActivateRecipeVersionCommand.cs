using MediatR;
using WMS.Application.Recipes.Dtos;
using WMS.Shared.Exceptions;
using WMS.Shared.Result;

namespace WMS.Application.Recipes.Commands;

public record ActivateRecipeVersionCommand(Guid RecipeId, Guid VersionId) : IRequest<Result<RecipeDto>>;

public class ActivateRecipeVersionHandler(IRecipeRepository repo)
    : IRequestHandler<ActivateRecipeVersionCommand, Result<RecipeDto>>
{
    public async Task<Result<RecipeDto>> Handle(ActivateRecipeVersionCommand request, CancellationToken ct)
    {
        var recipe = await repo.GetByIdWithVersionsAsync(request.RecipeId, ct);
        if (recipe is null)
            return Result.Failure<RecipeDto>("NOT_FOUND", "Reçete bulunamadı.");

        try
        {
            recipe.ActivateVersion(request.VersionId);
            await repo.SaveAsync(ct);
            return Result.Success(RecipeDto.FromEntity(recipe));
        }
        catch (BusinessException ex)
        {
            return Result.Failure<RecipeDto>(ex.Code, ex.Message);
        }
    }
}
