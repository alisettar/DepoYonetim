using MediatR;
using WMS.Application.Recipes.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Recipes.Queries;

public record GetRecipeQuery(Guid Id) : IRequest<Result<RecipeDto>>;

public class GetRecipeHandler(IRecipeRepository repo)
    : IRequestHandler<GetRecipeQuery, Result<RecipeDto>>
{
    public async Task<Result<RecipeDto>> Handle(GetRecipeQuery request, CancellationToken ct)
    {
        var recipe = await repo.GetByIdWithVersionsAsync(request.Id, ct);
        return recipe is not null
            ? Result.Success(RecipeDto.FromEntity(recipe))
            : Result.Failure<RecipeDto>("NOT_FOUND", "Reçete bulunamadı.");
    }
}
