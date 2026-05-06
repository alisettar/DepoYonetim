using WMS.Domain.Catalog;

namespace WMS.Infrastructure.Persistence.Seeds;

public static class UnitSeed
{
    public static readonly Guid AdetId   = Guid.Parse("10000000-0000-0000-0000-000000000001");
    public static readonly Guid KgId     = Guid.Parse("10000000-0000-0000-0000-000000000002");
    public static readonly Guid LitreId  = Guid.Parse("10000000-0000-0000-0000-000000000003");
    public static readonly Guid MetreId  = Guid.Parse("10000000-0000-0000-0000-000000000004");
    public static readonly Guid KutuId   = Guid.Parse("10000000-0000-0000-0000-000000000005");

    public static void Seed(AppDbContext db)
    {
        if (db.Units.Any()) return;

        db.Units.AddRange(
            Unit.Create(AdetId,  "ADET", "Adet"),
            Unit.Create(KgId,    "KG",   "Kilogram"),
            Unit.Create(LitreId, "LT",   "Litre"),
            Unit.Create(MetreId, "MT",   "Metre"),
            Unit.Create(KutuId,  "KTU",  "Kutu")
        );
        db.SaveChanges();
    }
}
