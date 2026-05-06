using MediatR;
using WMS.Application.Catalog.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Catalog.Products.Commands;

public record RemoveProductUnitCommand(Guid ProductId, Guid ProductUnitId) : IRequest<Result<ProductDto>>;

public class RemoveProductUnitHandler(IProductRepository repo) : IRequestHandler<RemoveProductUnitCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(RemoveProductUnitCommand request, CancellationToken ct)
    {
        var product = await repo.GetByIdAsync(request.ProductId, ct);
        if (product is null)
            return Result.Failure<ProductDto>("NOT_FOUND", "Ürün bulunamadı.");

        try
        {
            product.RemoveUnit(request.ProductUnitId);
            await repo.SaveAsync(ct);
            return Result.Success(ProductDto.FromEntity(product));
        }
        catch (Exception ex)
        {
            return Result.Failure<ProductDto>("BUSINESS_ERROR", ex.Message);
        }
    }
}
