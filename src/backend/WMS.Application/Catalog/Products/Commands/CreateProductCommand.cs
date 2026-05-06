using MediatR;
using WMS.Application.Catalog.Dtos;
using WMS.Domain.Catalog;
using WMS.Shared.Result;

namespace WMS.Application.Catalog.Products.Commands;

public record CreateProductCommand(
    string Code,
    string Name,
    Guid CategoryId,
    Guid PrimaryUnitId,
    bool LotRequired = false,
    int? ShelfLifeDays = null,
    decimal? MinStock = null,
    decimal? MaxStock = null) : IRequest<Result<ProductDto>>;

public class CreateProductHandler(IProductRepository repo) : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken ct)
    {
        if (await repo.CodeExistsAsync(request.Code, ct))
            return Result.Failure<ProductDto>("CODE_EXISTS", $"'{request.Code}' kodlu ürün zaten mevcut.");

        try
        {
            var product = Product.Create(
                Guid.NewGuid(), request.Code, request.Name,
                request.CategoryId, request.PrimaryUnitId,
                request.LotRequired, request.ShelfLifeDays,
                request.MinStock, request.MaxStock);

            repo.Add(product);
            await repo.SaveAsync(ct);
            return Result.Success(ProductDto.FromEntity(product));
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<ProductDto>("VALIDATION_ERROR", ex.Message);
        }
    }
}
