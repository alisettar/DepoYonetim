using MediatR;
using WMS.Application.Catalog.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Catalog.Categories.Queries;

public record ListCategoriesQuery : IRequest<Result<List<CategoryDto>>>;

public class ListCategoriesHandler(ICategoryRepository repo) : IRequestHandler<ListCategoriesQuery, Result<List<CategoryDto>>>
{
    public async Task<Result<List<CategoryDto>>> Handle(ListCategoriesQuery request, CancellationToken ct)
    {
        var categories = await repo.GetAllAsync(ct);
        return Result.Success(categories.Select(CategoryDto.FromEntity).ToList());
    }
}
