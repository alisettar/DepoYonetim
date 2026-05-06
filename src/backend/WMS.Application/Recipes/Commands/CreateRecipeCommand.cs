using MediatR;
using WMS.Application.Recipes.Dtos;
using WMS.Domain.Recipes;
using WMS.Shared.Result;

namespace WMS.Application.Recipes.Commands;

public record CreateRecipeCommand(Guid ProductId, string Name) : IRequest<Result<RecipeDto>>;

public class CreateRecipeHandler(IRecipeRepository repo) : IRequestHandler<CreateRecipeCommand, Result<RecipeDto>>
{
    public async Task<Result<RecipeDto>> Handle(CreateRecipeCommand request, CancellationToken ct)
    {
        try
        {
            var recipe = Recipe.Create(Guid.NewGuid(), request.ProductId, request.Name);
            repo.Add(recipe);
            await repo.SaveAsync(ct);
            return Result.Success(RecipeDto.FromEntity(recipe));
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<RecipeDto>("VALIDATION_ERROR", ex.Message);
        }
    }
}
