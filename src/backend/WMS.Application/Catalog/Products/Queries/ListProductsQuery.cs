using MediatR;
using WMS.Application.Catalog.Dtos;
using WMS.Domain.Catalog;
using WMS.Shared.Result;

namespace WMS.Application.Catalog.Products.Queries;

public record ListProductsQuery(ProductStatus? Status = null) : IRequest<Result<List<ProductDto>>>;

public class ListProductsHandler(IProductRepository repo) : IRequestHandler<ListProductsQuery, Result<List<ProductDto>>>
{
    public async Task<Result<List<ProductDto>>> Handle(ListProductsQuery request, CancellationToken ct)
    {
        var products = await repo.GetAllAsync(request.Status, ct);
        return Result.Success(products.Select(ProductDto.FromEntity).ToList());
    }
}
