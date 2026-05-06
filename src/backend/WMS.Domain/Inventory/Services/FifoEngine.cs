using WMS.Shared.Exceptions;

namespace WMS.Domain.Inventory.Services;

public static class FifoEngine
{
    public static IReadOnlyList<FifoConsumption> Consume(
        IReadOnlyList<FifoLayer> layers,
        decimal requestedQuantity)
    {
        var result = new List<FifoConsumption>();
        var remaining = requestedQuantity;

        foreach (var layer in layers)
        {
            if (remaining <= 0)
                break;

            var take = Math.Min(layer.RemainingQuantity, remaining);
            result.Add(new FifoConsumption(layer.Id, layer.LotId, take, layer.UnitCost, layer.ReceiptDate));
            remaining -= take;
        }

        if (remaining > 0)
            throw new BusinessException("INSUFFICIENT_STOCK", "Yetersiz stok. Talep edilen miktar mevcut stoktan fazla.");

        return result;
    }
}
