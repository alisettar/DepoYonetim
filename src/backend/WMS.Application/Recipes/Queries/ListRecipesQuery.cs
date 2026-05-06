using MediatR;
using WMS.Application.Recipes.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Recipes.Queries;

public record ListRecipesQuery(Guid? ProductId = null) : IRequest<Result<List<RecipeSummaryDto>>>;

public class ListRecipesHandler(IRecipeRepository repo)
    : IRequestHandler<ListRecipesQuery, Result<List<RecipeSummaryDto>>>
{
    public async Task<Result<List<RecipeSummaryDto>>> Handle(ListRecipesQuery request, CancellationToken ct)
    {
        var recipes = await repo.GetAllAsync(ct);

        if (request.ProductId.HasValue)
            recipes = recipes.Where(r => r.ProductId == request.ProductId.Value).ToList();

        return Result.Success(recipes.Select(RecipeSummaryDto.FromEntity).ToList());
    }
}
