using MediatR;
using WMS.Application.Catalog.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Catalog.Products.Queries;

public record GetProductQuery(Guid Id) : IRequest<Result<ProductDto>>;

public class GetProductHandler(IProductRepository repo) : IRequestHandler<GetProductQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(GetProductQuery request, CancellationToken ct)
    {
        var product = await repo.GetByIdAsync(request.Id, ct);
        return product is null
            ? Result.Failure<ProductDto>("NOT_FOUND", "Ürün bulunamadı.")
            : Result.Success(ProductDto.FromEntity(product));
    }
}
