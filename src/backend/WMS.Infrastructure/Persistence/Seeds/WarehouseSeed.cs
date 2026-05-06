using WMS.Domain.Warehousing;

namespace WMS.Infrastructure.Persistence.Seeds;

public static class WarehouseSeed
{
    public static void Seed(AppDbContext db)
    {
        if (db.Warehouses.Any()) return;

        var w1 = Warehouse.Create(
            Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
            "ANA-DEPO", "Ana Depo", WarehouseType.Physical, "İstanbul");
        w1.CreateLocation("A-01", "Koridor A - Raf 1");
        w1.CreateLocation("A-02", "Koridor A - Raf 2");
        w1.CreateLocation("B-01", "Koridor B - Raf 1", capacity: 500);
        db.Warehouses.Add(w1);

        var w2 = Warehouse.Create(
            Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
            "KARANT", "Karantina Deposu", WarehouseType.Virtual);
        db.Warehouses.Add(w2);

        var m1 = MachineWarehouse.Create(
            Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012"),
            "MK-001", "Torna Makinesi Deposu", "TORNA-001", "Üretim Sahası");
        db.Warehouses.Add(m1);

        db.SaveChanges();
    }
}
