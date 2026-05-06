using WMS.Shared.Exceptions;

namespace WMS.Domain.Recipes;

public class RecipeItem
{
    public Guid Id { get; private set; }
    public Guid RecipeVersionId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal Quantity { get; private set; }
    public Guid UnitId { get; private set; }
    public decimal? WastePercent { get; private set; }
    public decimal? WasteFixed { get; private set; }
    public int SortOrder { get; private set; }

    private readonly List<AlternativeMaterial> _alternatives = [];
    public IReadOnlyCollection<AlternativeMaterial> Alternatives => _alternatives.AsReadOnly();

    private RecipeItem() { }

    public static RecipeItem Create(
        Guid recipeVersionId,
        Guid productId,
        decimal quantity,
        Guid unitId,
        decimal? wastePercent = null,
        decimal? wasteFixed = null,
        int sortOrder = 0)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Ürün zorunludur.", nameof(productId));
        if (quantity <= 0)
            throw new ArgumentException("Miktar sıfırdan büyük olmalıdır.", nameof(quantity));
        if (unitId == Guid.Empty)
            throw new ArgumentException("Birim zorunludur.", nameof(unitId));
        if (wastePercent.HasValue && (wastePercent < 0 || wastePercent > 100))
            throw new ArgumentException("Fire yüzdesi 0-100 arasında olmalıdır.", nameof(wastePercent));
        if (wasteFixed.HasValue && wasteFixed < 0)
            throw new ArgumentException("Sabit fire negatif olamaz.", nameof(wasteFixed));

        return new RecipeItem
        {
            Id = Guid.NewGuid(),
            RecipeVersionId = recipeVersionId,
            ProductId = productId,
            Quantity = quantity,
            UnitId = unitId,
            WastePercent = wastePercent,
            WasteFixed = wasteFixed,
            SortOrder = sortOrder
        };
    }

    public void Update(decimal quantity, Guid unitId, decimal? wastePercent, decimal? wasteFixed, int sortOrder)
    {
        if (quantity <= 0)
            throw new ArgumentException("Miktar sıfırdan büyük olmalıdır.", nameof(quantity));
        if (unitId == Guid.Empty)
            throw new ArgumentException("Birim zorunludur.", nameof(unitId));
        if (wastePercent.HasValue && (wastePercent < 0 || wastePercent > 100))
            throw new ArgumentException("Fire yüzdesi 0-100 arasında olmalıdır.", nameof(wastePercent));
        if (wasteFixed.HasValue && wasteFixed < 0)
            throw new ArgumentException("Sabit fire negatif olamaz.", nameof(wasteFixed));

        Quantity = quantity;
        UnitId = unitId;
        WastePercent = wastePercent;
        WasteFixed = wasteFixed;
        SortOrder = sortOrder;
    }

    public AlternativeMaterial AddAlternative(Guid productId, int priority, decimal quantity, Guid unitId)
    {
        if (productId == ProductId)
            throw new BusinessException("ALT_SAME_AS_MAIN", "Alternatif malzeme ana malzeme ile aynı olamaz.");
        if (_alternatives.Any(a => a.ProductId == productId))
            throw new BusinessException("ALT_ALREADY_EXISTS", "Bu ürün zaten alternatif olarak tanımlı.");

        var alt = AlternativeMaterial.Create(Id, productId, priority, quantity, unitId);
        _alternatives.Add(alt);
        return alt;
    }

    public void RemoveAlternative(Guid alternativeId)
    {
        var alt = _alternatives.FirstOrDefault(a => a.Id == alternativeId)
            ?? throw new BusinessException("ALT_NOT_FOUND", "Alternatif malzeme bulunamadı.");
        _alternatives.Remove(alt);
    }
}
