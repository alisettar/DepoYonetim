using MediatR;
using WMS.Application.Catalog.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Catalog.Products.Commands;

public record AddProductUnitCommand(Guid ProductId, Guid UnitId, decimal ConversionToPrimary)
    : IRequest<Result<ProductDto>>;

public class AddProductUnitHandler(IProductRepository repo) : IRequestHandler<AddProductUnitCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(AddProductUnitCommand request, CancellationToken ct)
    {
        var product = await repo.GetByIdAsync(request.ProductId, ct);
        if (product is null)
            return Result.Failure<ProductDto>("NOT_FOUND", "Ürün bulunamadı.");

        try
        {
            product.AddUnit(request.UnitId, request.ConversionToPrimary);
            await repo.SaveAsync(ct);
            return Result.Success(ProductDto.FromEntity(product));
        }
        catch (Exception ex)
        {
            return Result.Failure<ProductDto>("BUSINESS_ERROR", ex.Message);
        }
    }
}
