using WMS.Domain.Catalog;

namespace WMS.Application.Catalog.Dtos;

public record ProductUnitDto(Guid Id, Guid UnitId, decimal ConversionToPrimary)
{
    public static ProductUnitDto FromEntity(ProductUnit pu) =>
        new(pu.Id, pu.UnitId, pu.ConversionToPrimary);
}

public record ProductDto(
    Guid Id,
    string Code,
    string Name,
    Guid CategoryId,
    Guid PrimaryUnitId,
    bool LotRequired,
    int? ShelfLifeDays,
    decimal? MinStock,
    decimal? MaxStock,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyCollection<ProductUnitDto> Units)
{
    public static ProductDto FromEntity(Product p) =>
        new(p.Id, p.Code, p.Name, p.CategoryId, p.PrimaryUnitId,
            p.LotRequired, p.ShelfLifeDays, p.MinStock, p.MaxStock,
            p.Status.ToString(), p.CreatedAt, p.UpdatedAt,
            p.Units.Select(ProductUnitDto.FromEntity).ToList());
}
