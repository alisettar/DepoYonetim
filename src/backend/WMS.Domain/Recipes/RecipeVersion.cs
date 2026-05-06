using WMS.Shared.Exceptions;

namespace WMS.Domain.Recipes;

public class RecipeVersion
{
    public Guid Id { get; private set; }
    public Guid RecipeId { get; private set; }
    public int VersionNo { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime? ValidUntil { get; private set; }
    public bool IsActive { get; private set; }
    public decimal OutputQuantity { get; private set; }
    public Guid OutputUnitId { get; private set; }

    private readonly List<RecipeItem> _items = [];
    public IReadOnlyCollection<RecipeItem> Items => _items.AsReadOnly();

    private RecipeVersion() { }

    public static RecipeVersion Create(
        Guid recipeId,
        int versionNo,
        DateTime validFrom,
        DateTime? validUntil,
        decimal outputQuantity,
        Guid outputUnitId)
    {
        if (outputQuantity <= 0)
            throw new ArgumentException("Çıkış miktarı sıfırdan büyük olmalıdır.", nameof(outputQuantity));
        if (outputUnitId == Guid.Empty)
            throw new ArgumentException("Çıkış birimi zorunludur.", nameof(outputUnitId));
        if (validUntil.HasValue && validUntil <= validFrom)
            throw new ArgumentException("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.", nameof(validUntil));

        return new RecipeVersion
        {
            Id = Guid.NewGuid(),
            RecipeId = recipeId,
            VersionNo = versionNo,
            ValidFrom = validFrom,
            ValidUntil = validUntil,
            IsActive = false,
            OutputQuantity = outputQuantity,
            OutputUnitId = outputUnitId
        };
    }

    public void Update(DateTime validFrom, DateTime? validUntil, decimal outputQuantity, Guid outputUnitId)
    {
        if (IsActive)
            throw new BusinessException("Aktif versiyon güncellenemez.", "VERSION_IS_ACTIVE");
        if (outputQuantity <= 0)
            throw new ArgumentException("Çıkış miktarı sıfırdan büyük olmalıdır.", nameof(outputQuantity));
        if (outputUnitId == Guid.Empty)
            throw new ArgumentException("Çıkış birimi zorunludur.", nameof(outputUnitId));
        if (validUntil.HasValue && validUntil <= validFrom)
            throw new ArgumentException("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.", nameof(validUntil));

        ValidFrom = validFrom;
        ValidUntil = validUntil;
        OutputQuantity = outputQuantity;
        OutputUnitId = outputUnitId;
    }

    internal void Activate() => IsActive = true;
    internal void Deactivate() => IsActive = false;

    public RecipeItem AddItem(
        Guid productId,
        decimal quantity,
        Guid unitId,
        decimal? wastePercent = null,
        decimal? wasteFixed = null,
        int sortOrder = 0)
    {
        if (_items.Any(i => i.ProductId == productId))
            throw new BusinessException("Bu ürün zaten reçete kaleminde mevcut.", "ITEM_ALREADY_EXISTS");

        var item = RecipeItem.Create(Id, productId, quantity, unitId, wastePercent, wasteFixed, sortOrder);
        _items.Add(item);
        return item;
    }

    public void RemoveItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new BusinessException("Reçete kalemi bulunamadı.", "ITEM_NOT_FOUND");
        _items.Remove(item);
    }

    public RecipeItem GetItem(Guid itemId) =>
        _items.FirstOrDefault(i => i.Id == itemId)
        ?? throw new BusinessException("Reçete kalemi bulunamadı.", "ITEM_NOT_FOUND");
}
