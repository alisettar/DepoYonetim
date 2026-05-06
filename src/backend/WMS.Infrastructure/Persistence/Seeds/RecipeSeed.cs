using WMS.Domain.Catalog;
using WMS.Domain.Recipes;

namespace WMS.Infrastructure.Persistence.Seeds;

public static class RecipeSeed
{
    public static void Seed(AppDbContext db)
    {
        if (db.Recipes.Any()) return;

        var tabla = db.Products.First(p => p.Code == "MMR-TABLA-001");
        var celik = db.Products.First(p => p.Code == "HMD-CELIK-001");
        var boya = db.Products.First(p => p.Code == "HMD-BOYA-001");

        // Tabla için örnek reçete
        var recipe = Recipe.Create(
            Guid.Parse("40000000-0000-0000-0000-000000000001"),
            tabla.Id, "Metal Tabla Üretim Reçetesi");

        var version = recipe.AddVersion(
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            null,
            1,
            UnitSeed.AdetId);

        // Kalem 1: Çelik Sac
        version.AddItem(celik.Id, 10, UnitSeed.KgId, wastePercent: 5, sortOrder: 1);

        // Kalem 2: Boya
        version.AddItem(boya.Id, 2, UnitSeed.LitreId, wastePercent: 10, sortOrder: 2);

        // Kalem 3: Civata
        var civata = db.Products.First(p => p.Code == "YRD-CIVATA-M8");
        version.AddItem(civata.Id, 8, UnitSeed.AdetId, sortOrder: 3);

        db.Recipes.Add(recipe);
        db.SaveChanges();
    }
}
