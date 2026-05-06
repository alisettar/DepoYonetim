using WMS.Domain.Inventory.Enums;

namespace WMS.Domain.Inventory;

public class StockMovement
{
    public Guid Id { get; protected set; }
    public DateTime OccurredAt { get; protected set; }
    public MovementType Type { get; protected set; }
    public Guid ProductId { get; protected set; }
    public Guid? LotId { get; protected set; }
    public Guid? WarehouseId { get; protected set; }
    public Guid? MachineWarehouseId { get; protected set; }
    public Guid? LocationId { get; protected set; }
    public decimal Quantity { get; protected set; }
    public Guid UnitId { get; protected set; }
    public decimal? UnitCost { get; protected set; }
    public string? ReferenceType { get; protected set; }
    public Guid? ReferenceId { get; protected set; }
    public Guid? ReversalOfId { get; protected set; }
    public bool IsVoided { get; protected set; } = false;
    public DateTime? VoidedAt { get; protected set; }
    public Guid? VoidedByUserId { get; protected set; }
    public string? VoidReason { get; protected set; }
    public string? Note { get; protected set; }
    public Guid CreatedByUserId { get; protected set; }
    public DateTime CreatedAt { get; protected set; }

    protected StockMovement() { }

    public static StockMovement CreateGoodsReceipt(
        Guid productId,
        Guid? lotId,
        Guid warehouseId,
        Guid? locationId,
        decimal quantity,
        Guid unitId,
        decimal? unitCost,
        Guid createdByUserId,
        string? note = null,
        Guid? referenceId = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Miktar sıfırdan büyük olmalıdır.", nameof(quantity));

        return new StockMovement
        {
            Id = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            Type = MovementType.GoodsReceipt,
            ProductId = productId,
            LotId = lotId,
            WarehouseId = warehouseId,
            LocationId = locationId,
            Quantity = quantity,
            UnitId = unitId,
            UnitCost = unitCost,
            Note = note,
            ReferenceId = referenceId,
            CreatedByUserId = createdByUserId
        };
    }

    public static StockMovement CreateShipment(
        Guid productId,
        Guid? lotId,
        Guid warehouseId,
        decimal quantity,
        Guid unitId,
        decimal? unitCost,
        Guid createdByUserId,
        string? note = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Miktar sıfırdan büyük olmalıdır.", nameof(quantity));

        return new StockMovement
        {
            Id = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            Type = MovementType.Shipment,
            ProductId = productId,
            LotId = lotId,
            WarehouseId = warehouseId,
            Quantity = -quantity,
            UnitId = unitId,
            UnitCost = unitCost,
            Note = note,
            CreatedByUserId = createdByUserId
        };
    }

    public static StockMovement CreateTransferOut(
        Guid productId,
        Guid? lotId,
        Guid warehouseId,
        decimal quantity,
        Guid unitId,
        decimal? unitCost,
        Guid createdByUserId)
    {
        if (quantity <= 0)
            throw new ArgumentException("Miktar sıfırdan büyük olmalıdır.", nameof(quantity));

        return new StockMovement
        {
            Id = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            Type = MovementType.TransferOut,
            ProductId = productId,
            LotId = lotId,
            WarehouseId = warehouseId,
            Quantity = -quantity,
            UnitId = unitId,
            UnitCost = unitCost,
            CreatedByUserId = createdByUserId
        };
    }

    public static StockMovement CreateTransferIn(
        Guid productId,
        Guid? lotId,
        Guid warehouseId,
        decimal quantity,
        Guid unitId,
        decimal? unitCost,
        Guid createdByUserId)
    {
        if (quantity <= 0)
            throw new ArgumentException("Miktar sıfırdan büyük olmalıdır.", nameof(quantity));

        return new StockMovement
        {
            Id = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            Type = MovementType.TransferIn,
            ProductId = productId,
            LotId = lotId,
            WarehouseId = warehouseId,
            Quantity = quantity,
            UnitId = unitId,
            UnitCost = unitCost,
            CreatedByUserId = createdByUserId
        };
    }

    public static StockMovement CreateAdjustmentIn(
        Guid productId,
        Guid? lotId,
        Guid warehouseId,
        decimal quantity,
        Guid unitId,
        decimal? unitCost,
        string? reason,
        Guid createdByUserId)
    {
        if (quantity <= 0)
            throw new ArgumentException("Miktar sıfırdan büyük olmalıdır.", nameof(quantity));

        return new StockMovement
        {
            Id = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            Type = MovementType.InventoryAdjustmentIn,
            ProductId = productId,
            LotId = lotId,
            WarehouseId = warehouseId,
            Quantity = quantity,
            UnitId = unitId,
            UnitCost = unitCost,
            Note = reason,
            CreatedByUserId = createdByUserId
        };
    }

    public static StockMovement CreateAdjustmentOut(
        Guid productId,
        Guid? lotId,
        Guid warehouseId,
        decimal quantity,
        Guid unitId,
        Guid createdByUserId,
        string? reason)
    {
        if (quantity <= 0)
            throw new ArgumentException("Miktar sıfırdan büyük olmalıdır.", nameof(quantity));

        return new StockMovement
        {
            Id = Guid.NewGuid(),
            OccurredAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            Type = MovementType.InventoryAdjustmentOut,
            ProductId = productId,
            LotId = lotId,
            WarehouseId = warehouseId,
            Quantity = -quantity,
            UnitId = unitId,
            Note = reason,
            CreatedByUserId = createdByUserId
        };
    }
}
