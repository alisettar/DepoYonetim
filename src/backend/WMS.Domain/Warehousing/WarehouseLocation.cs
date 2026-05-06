namespace WMS.Domain.Warehousing;

public class WarehouseLocation
{
    public Guid Id { get; private set; }
    public Guid WarehouseId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public decimal? Capacity { get; private set; }
    public bool IsActive { get; private set; }

    private WarehouseLocation() { }

    public static WarehouseLocation Create(
        Guid warehouseId,
        string code,
        string name,
        decimal? capacity = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Konum kodu zorunludur.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Konum adı zorunludur.", nameof(name));
        if (code.Length > 20)
            throw new ArgumentException("Konum kodu en fazla 20 karakter olabilir.", nameof(code));
        if (capacity.HasValue && capacity.Value < 0)
            throw new ArgumentException("Kapasite negatif olamaz.", nameof(capacity));

        return new WarehouseLocation
        {
            Id = Guid.NewGuid(),
            WarehouseId = warehouseId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Capacity = capacity,
            IsActive = true
        };
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
