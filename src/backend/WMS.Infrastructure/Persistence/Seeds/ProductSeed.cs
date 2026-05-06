using WMS.Domain.Catalog;

namespace WMS.Infrastructure.Persistence.Seeds;

public static class ProductSeed
{
    public static void Seed(AppDbContext db)
    {
        if (db.Products.Any()) return;

        var celik = Product.Create(
            Guid.Parse("30000000-0000-0000-0000-000000000001"),
            "HMD-CELIK-001", "Çelik Sac 2mm",
            CategorySeed.HammaddeId, UnitSeed.KgId,
            lotRequired: true, minStock: 100, maxStock: 5000);
        celik.AddUnit(UnitSeed.AdetId, 5);

        var boya = Product.Create(
            Guid.Parse("30000000-0000-0000-0000-000000000002"),
            "HMD-BOYA-001", "Endüstriyel Boya 15L",
            CategorySeed.HammaddeId, UnitSeed.LitreId,
            lotRequired: false, shelfLifeDays: 730, minStock: 50, maxStock: 500);

        var tabla = Product.Create(
            Guid.Parse("30000000-0000-0000-0000-000000000003"),
            "MMR-TABLA-001", "Metal Tabla",
            CategorySeed.MamulId, UnitSeed.AdetId,
            lotRequired: true, minStock: 10, maxStock: 200);

        var civata = Product.Create(
            Guid.Parse("30000000-0000-0000-0000-000000000004"),
            "YRD-CIVATA-M8", "Civata M8",
            CategorySeed.YardimciMalzemeId, UnitSeed.AdetId,
            lotRequired: false, minStock: 500, maxStock: 10000);
        civata.AddUnit(UnitSeed.KutuId, 100);

        db.Products.AddRange(celik, boya, tabla, civata);
        db.SaveChanges();
    }
}
