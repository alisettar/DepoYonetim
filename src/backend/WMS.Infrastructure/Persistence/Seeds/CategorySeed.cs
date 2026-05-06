using WMS.Domain.Catalog;

namespace WMS.Infrastructure.Persistence.Seeds;

public static class CategorySeed
{
    public static readonly Guid HammaddeId       = Guid.Parse("20000000-0000-0000-0000-000000000001");
    public static readonly Guid YariMamulId      = Guid.Parse("20000000-0000-0000-0000-000000000002");
    public static readonly Guid MamulId          = Guid.Parse("20000000-0000-0000-0000-000000000003");
    public static readonly Guid YardimciMalzemeId = Guid.Parse("20000000-0000-0000-0000-000000000004");

    public static void Seed(AppDbContext db)
    {
        if (db.Categories.Any()) return;

        db.Categories.AddRange(
            Category.Create(HammaddeId,        "HMD", "Hammadde"),
            Category.Create(YariMamulId,       "YMM", "Yarı Mamul"),
            Category.Create(MamulId,           "MMR", "Mamul"),
            Category.Create(YardimciMalzemeId, "YRD", "Yardımcı Malzeme")
        );
        db.SaveChanges();
    }
}
