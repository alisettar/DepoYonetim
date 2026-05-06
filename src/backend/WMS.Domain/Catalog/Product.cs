using WMS.Shared.Exceptions;

namespace WMS.Domain.Catalog;

public class Product
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Guid CategoryId { get; private set; }
    public Guid PrimaryUnitId { get; private set; }
    public bool LotRequired { get; private set; }
    public int? ShelfLifeDays { get; private set; }
    public decimal? MinStock { get; private set; }
    public decimal? MaxStock { get; private set; }
    public ProductStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<ProductUnit> _units = [];
    public IReadOnlyCollection<ProductUnit> Units => _units.AsReadOnly();

    private Product() { }

    public static Product Create(
        Guid id,
        string code,
        string name,
        Guid categoryId,
        Guid primaryUnitId,
        bool lotRequired = false,
        int? shelfLifeDays = null,
        decimal? minStock = null,
        decimal? maxStock = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Ürün kodu zorunludur.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Ürün adı zorunludur.", nameof(name));
        if (code.Length > 50)
            throw new ArgumentException("Ürün kodu en fazla 50 karakter olabilir.", nameof(code));
        if (name.Length > 200)
            throw new ArgumentException("Ürün adı en fazla 200 karakter olabilir.", nameof(name));
        if (categoryId == Guid.Empty)
            throw new ArgumentException("Kategori zorunludur.", nameof(categoryId));
        if (primaryUnitId == Guid.Empty)
            throw new ArgumentException("Birincil birim zorunludur.", nameof(primaryUnitId));
        if (shelfLifeDays.HasValue && shelfLifeDays <= 0)
            throw new ArgumentException("Raf ömrü sıfırdan büyük olmalıdır.", nameof(shelfLifeDays));
        if (minStock.HasValue && minStock < 0)
            throw new ArgumentException("Minimum stok negatif olamaz.", nameof(minStock));
        if (maxStock.HasValue && maxStock < 0)
            throw new ArgumentException("Maksimum stok negatif olamaz.", nameof(maxStock));
        if (minStock.HasValue && maxStock.HasValue && minStock > maxStock)
            throw new ArgumentException("Minimum stok maksimum stoktan büyük olamaz.", nameof(minStock));

        var now = DateTime.UtcNow;
        return new Product
        {
            Id = id,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            CategoryId = categoryId,
            PrimaryUnitId = primaryUnitId,
            LotRequired = lotRequired,
            ShelfLifeDays = shelfLifeDays,
            MinStock = minStock,
            MaxStock = maxStock,
            Status = ProductStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Update(
        string name,
        Guid categoryId,
        bool lotRequired,
        int? shelfLifeDays,
        decimal? minStock,
        decimal? maxStock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Ürün adı zorunludur.", nameof(name));
        if (name.Length > 200)
            throw new ArgumentException("Ürün adı en fazla 200 karakter olabilir.", nameof(name));
        if (categoryId == Guid.Empty)
            throw new ArgumentException("Kategori zorunludur.", nameof(categoryId));
        if (shelfLifeDays.HasValue && shelfLifeDays <= 0)
            throw new ArgumentException("Raf ömrü sıfırdan büyük olmalıdır.", nameof(shelfLifeDays));
        if (minStock.HasValue && minStock < 0)
            throw new ArgumentException("Minimum stok negatif olamaz.", nameof(minStock));
        if (maxStock.HasValue && maxStock < 0)
            throw new ArgumentException("Maksimum stok negatif olamaz.", nameof(maxStock));
        if (minStock.HasValue && maxStock.HasValue && minStock > maxStock)
            throw new ArgumentException("Minimum stok maksimum stoktan büyük olamaz.", nameof(minStock));

        Name = name.Trim();
        CategoryId = categoryId;
        LotRequired = lotRequired;
        ShelfLifeDays = shelfLifeDays;
        MinStock = minStock;
        MaxStock = maxStock;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = ProductStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = ProductStatus.Passive;
        UpdatedAt = DateTime.UtcNow;
    }

    public ProductUnit AddUnit(Guid unitId, decimal conversionToPrimary)
    {
        if (unitId == PrimaryUnitId)
            throw new BusinessException("UNIT_IS_PRIMARY", "Birincil birim ek birim olarak eklenemez.");
        if (_units.Any(u => u.UnitId == unitId))
            throw new BusinessException("UNIT_ALREADY_EXISTS", "Bu birim zaten tanımlı.");

        var pu = ProductUnit.Create(Id, unitId, conversionToPrimary);
        _units.Add(pu);
        UpdatedAt = DateTime.UtcNow;
        return pu;
    }

    public void RemoveUnit(Guid productUnitId)
    {
        var pu = _units.FirstOrDefault(u => u.Id == productUnitId)
            ?? throw new BusinessException("PRODUCT_UNIT_NOT_FOUND", "Ürün birimi bulunamadı.");
        _units.Remove(pu);
        UpdatedAt = DateTime.UtcNow;
    }
}
