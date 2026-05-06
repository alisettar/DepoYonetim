using WMS.Domain.Inventory;
using WMS.Domain.Inventory.Enums;

namespace WMS.Infrastructure.Persistence.Seeds;

public static class InventorySeed
{
    public static void Seed(AppDbContext db)
    {
        if (db.StockMovements.Any()) return;

        var warehouseId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        var celikId     = Guid.Parse("30000000-0000-0000-0000-000000000001");
        var boyaId      = Guid.Parse("30000000-0000-0000-0000-000000000002");
        var tablaId     = Guid.Parse("30000000-0000-0000-0000-000000000003");
        var civataId    = Guid.Parse("30000000-0000-0000-0000-000000000004");
        var kgId        = Guid.Parse("10000000-0000-0000-0000-000000000002");
        var litreId     = Guid.Parse("10000000-0000-0000-0000-000000000003");
        var adetId      = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var sysUser     = Guid.Empty;
        var receiptDate = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        // Lots
        var celikLot = Lot.Create(
            Guid.Parse("50000000-0000-0000-0000-000000000001"),
            celikId, "L-2024-001", LotSource.Receipt,
            new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc));

        var tablaLot = Lot.Create(
            Guid.Parse("50000000-0000-0000-0000-000000000002"),
            tablaId, "L-2024-002", LotSource.Receipt,
            new DateTime(2024, 3, 10, 0, 0, 0, DateTimeKind.Utc));

        db.Lots.AddRange(celikLot, tablaLot);

        // GoodsReceipt movements
        var m1 = StockMovement.CreateGoodsReceipt(celikId, celikLot.Id, warehouseId, null, 500, kgId, 12.50m, sysUser, "İlk stok");
        var m2 = StockMovement.CreateGoodsReceipt(boyaId, null, warehouseId, null, 120, litreId, 45.00m, sysUser, "İlk stok");
        var m3 = StockMovement.CreateGoodsReceipt(tablaId, tablaLot.Id, warehouseId, null, 50, adetId, 250.00m, sysUser, "İlk stok");
        var m4 = StockMovement.CreateGoodsReceipt(civataId, null, warehouseId, null, 2000, adetId, 0.50m, sysUser, "İlk stok");
        db.StockMovements.AddRange(m1, m2, m3, m4);

        // Balances
        db.StockBalances.AddRange(
            StockBalance.Create(celikId, celikLot.Id, warehouseId, null, 500),
            StockBalance.Create(boyaId, null, warehouseId, null, 120),
            StockBalance.Create(tablaId, tablaLot.Id, warehouseId, null, 50),
            StockBalance.Create(civataId, null, warehouseId, null, 2000)
        );

        // FifoLayers
        db.FifoLayers.AddRange(
            FifoLayer.Create(celikId, warehouseId, null, null, celikLot.Id, receiptDate, 500, 12.50m, m1.Id),
            FifoLayer.Create(boyaId, warehouseId, null, null, null, receiptDate, 120, 45.00m, m2.Id),
            FifoLayer.Create(tablaId, warehouseId, null, null, tablaLot.Id, receiptDate, 50, 250.00m, m3.Id),
            FifoLayer.Create(civataId, warehouseId, null, null, null, receiptDate, 2000, 0.50m, m4.Id)
        );

        db.SaveChanges();
    }
}
