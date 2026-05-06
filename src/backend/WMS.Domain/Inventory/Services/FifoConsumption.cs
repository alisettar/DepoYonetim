namespace WMS.Domain.Inventory.Services;

public record FifoConsumption(Guid LayerId, Guid? LotId, decimal Quantity, decimal? UnitCost, DateTime ReceiptDate);
