namespace WMS.Domain.Inventory.Enums;

public enum MovementType
{
    GoodsReceipt,
    Shipment,
    TransferIn,
    TransferOut,
    MachineLoad,
    MachineUnload,
    ProductionConsumption,
    ProductionOutput,
    InventoryAdjustmentIn,
    InventoryAdjustmentOut,
    Scrap,
    CustomerReturn,
    SupplierReturn,
    Reversal
}
