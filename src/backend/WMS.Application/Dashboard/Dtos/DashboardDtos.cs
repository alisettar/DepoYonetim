namespace WMS.Application.Dashboard.Dtos;

public record CriticalStockItem(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    Guid UnitId,
    string UnitName,
    decimal CurrentQty,
    decimal? MinStock,
    double FillRatio)
{
    public double CriticalRatio => MinStock.HasValue && MinStock.Value > 0
        ? Math.Clamp((double)(CurrentQty / MinStock.Value), 0.0, double.MaxValue)
        : 0;
}

public record WarehouseFillItem(
    Guid WarehouseId,
    string WarehouseName,
    Guid LocationId,
    string LocationCode,
    string LocationName,
    decimal FilledQuantity,
    decimal? Capacity,
    double? FillPercent)
{
    public double? FillRatio => Capacity.HasValue && Capacity.Value > 0
        ? (double?)Math.Clamp((double)(FilledQuantity / Capacity.Value), 0.0, 1.0)
        : null;
}

public record RecentMovementDto(
    Guid Id,
    DateTime OccurredAt,
    string Type,
    string ProductCode,
    string ProductName,
    decimal Quantity,
    string UnitName,
    Guid? LotId,
    string? LotNumber,
    string? WarehouseName,
    string? Note,
    Guid CreatedByUserId);

public record LotSearchItem(
    Guid Id,
    string LotNumber,
    Guid ProductId,
    string ProductCode,
    string ProductName,
    string QualityStatus,
    DateTime? ExpiryDate,
    DateTime ProductionDate,
    decimal? AvailableQty);
