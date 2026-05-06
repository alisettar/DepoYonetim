namespace WMS.Infrastructure.Persistence.Seeds;

public static class DatabaseSeeder
{
    public static void Seed(AppDbContext db)
    {
        WarehouseSeed.Seed(db);
        UnitSeed.Seed(db);
        CategorySeed.Seed(db);
        ProductSeed.Seed(db);
        InventorySeed.Seed(db);
    }
}
