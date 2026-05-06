using MediatR;
using WMS.Application.Recipes.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Recipes.Commands;

public record UpdateRecipeCommand(Guid Id, string Name) : IRequest<Result<RecipeDto>>;

public class UpdateRecipeHandler(IRecipeRepository repo) : IRequestHandler<UpdateRecipeCommand, Result<RecipeDto>>
{
    public async Task<Result<RecipeDto>> Handle(UpdateRecipeCommand request, CancellationToken ct)
    {
        var recipe = await repo.GetByIdWithVersionsAsync(request.Id, ct);
        if (recipe is null)
            return Result.Failure<RecipeDto>("NOT_FOUND", "Reçete bulunamadı.");

        try
        {
            recipe.Update(request.Name);
            await repo.SaveAsync(ct);
            return Result.Success(RecipeDto.FromEntity(recipe));
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<RecipeDto>("VALIDATION_ERROR", ex.Message);
        }
    }
}
