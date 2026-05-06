using WMS.Domain.Inventory;

namespace WMS.Application.Inventory.Dtos;

public record LotDto(
    Guid Id,
    Guid ProductId,
    string LotNumber,
    DateTime ProductionDate,
    DateTime? ExpiryDate,
    string Source,
    string QualityStatus,
    DateTime CreatedAt)
{
    public static LotDto FromEntity(Lot l) =>
        new(l.Id, l.ProductId, l.LotNumber, l.ProductionDate, l.ExpiryDate,
            l.Source.ToString(), l.QualityStatus.ToString(), l.CreatedAt);
}

public record StockMovementDto(
    Guid Id,
    DateTime OccurredAt,
    string Type,
    Guid ProductId,
    Guid? LotId,
    Guid? WarehouseId,
    Guid? MachineWarehouseId,
    Guid? LocationId,
    decimal Quantity,
    Guid UnitId,
    decimal? UnitCost,
    string? Note,
    Guid CreatedByUserId,
    DateTime CreatedAt)
{
    public static StockMovementDto FromEntity(StockMovement m) =>
        new(m.Id, m.OccurredAt, m.Type.ToString(), m.ProductId, m.LotId, m.WarehouseId,
            m.MachineWarehouseId, m.LocationId, m.Quantity, m.UnitId, m.UnitCost,
            m.Note, m.CreatedByUserId, m.CreatedAt);
}

public record StockBalanceDto(
    Guid Id,
    Guid ProductId,
    Guid? LotId,
    Guid? WarehouseId,
    Guid? MachineWarehouseId,
    decimal Quantity,
    DateTime UpdatedAt)
{
    public static StockBalanceDto FromEntity(StockBalance b) =>
        new(b.Id, b.ProductId, b.LotId, b.WarehouseId, b.MachineWarehouseId, b.Quantity, b.UpdatedAt);
}
