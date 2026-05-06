namespace WMS.Domain.Inventory;

public class StockBalance
{
    public Guid Id { get; protected set; }
    public Guid ProductId { get; protected set; }
    public Guid? LotId { get; protected set; }
    public Guid? WarehouseId { get; protected set; }
    public Guid? MachineWarehouseId { get; protected set; }
    public decimal Quantity { get; protected set; }
    public DateTime UpdatedAt { get; protected set; }

    protected StockBalance() { }

    public static StockBalance Create(
        Guid productId,
        Guid? lotId,
        Guid? warehouseId,
        Guid? machineWarehouseId,
        decimal initialQuantity)
    {
        return new StockBalance
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            LotId = lotId,
            WarehouseId = warehouseId,
            MachineWarehouseId = machineWarehouseId,
            Quantity = initialQuantity,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Apply(decimal delta)
    {
        Quantity += delta;
        UpdatedAt = DateTime.UtcNow;
    }
}
