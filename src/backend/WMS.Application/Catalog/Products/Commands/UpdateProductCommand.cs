using MediatR;
using WMS.Application.Catalog.Dtos;
using WMS.Shared.Result;

namespace WMS.Application.Catalog.Products.Commands;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    Guid CategoryId,
    bool LotRequired,
    int? ShelfLifeDays,
    decimal? MinStock,
    decimal? MaxStock) : IRequest<Result<ProductDto>>;

public class UpdateProductHandler(IProductRepository repo) : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await repo.GetByIdAsync(request.Id, ct);
        if (product is null)
            return Result.Failure<ProductDto>("NOT_FOUND", "Ürün bulunamadı.");

        try
        {
            product.Update(request.Name, request.CategoryId, request.LotRequired,
                request.ShelfLifeDays, request.MinStock, request.MaxStock);
            await repo.SaveAsync(ct);
            return Result.Success(ProductDto.FromEntity(product));
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<ProductDto>("VALIDATION_ERROR", ex.Message);
        }
    }
}
