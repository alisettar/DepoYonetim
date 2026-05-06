using MediatR;
using WMS.Application.Catalog.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Catalog.Categories.Queries;

public record GetCategoryQuery(Guid Id) : IRequest<Result<CategoryDto>>;

public class GetCategoryHandler(ICategoryRepository repo) : IRequestHandler<GetCategoryQuery, Result<CategoryDto>>
{
    public async Task<Result<CategoryDto>> Handle(GetCategoryQuery request, CancellationToken ct)
    {
        var category = await repo.GetByIdAsync(request.Id, ct);
        return category is null
            ? Result.Failure<CategoryDto>("NOT_FOUND", "Kategori bulunamadı.")
            : Result.Success(CategoryDto.FromEntity(category));
    }
}
