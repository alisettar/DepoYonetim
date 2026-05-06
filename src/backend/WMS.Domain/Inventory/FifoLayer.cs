namespace WMS.Domain.Inventory;

public class FifoLayer
{
    public Guid Id { get; protected set; }
    public Guid ProductId { get; protected set; }
    public Guid? WarehouseId { get; protected set; }
    public Guid? MachineWarehouseId { get; protected set; }
    public Guid? LocationId { get; protected set; }
    public Guid? LotId { get; protected set; }
    public DateTime ReceiptDate { get; protected set; }
    public decimal RemainingQuantity { get; protected set; }
    public decimal? UnitCost { get; protected set; }
    public Guid SourceMovementId { get; protected set; }
    public bool IsClosed { get; protected set; }

    protected FifoLayer() { }

    public static FifoLayer Create(
        Guid productId,
        Guid? warehouseId,
        Guid? machineWarehouseId,
        Guid? locationId,
        Guid? lotId,
        DateTime receiptDate,
        decimal quantity,
        decimal? unitCost,
        Guid sourceMovementId)
    {
        return new FifoLayer
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            MachineWarehouseId = machineWarehouseId,
            LocationId = locationId,
            LotId = lotId,
            ReceiptDate = NormalizeUtc(receiptDate),
            RemainingQuantity = quantity,
            UnitCost = unitCost,
            SourceMovementId = sourceMovementId,
            IsClosed = false
        };
    }

    public decimal Consume(decimal requestedQty)
    {
        var taken = Math.Min(RemainingQuantity, requestedQty);
        RemainingQuantity -= taken;
        if (RemainingQuantity == 0)
            IsClosed = true;
        return taken;
    }

    private static DateTime NormalizeUtc(DateTime dt) =>
        dt.Kind != DateTimeKind.Utc ? DateTime.SpecifyKind(dt, DateTimeKind.Utc) : dt;
}
