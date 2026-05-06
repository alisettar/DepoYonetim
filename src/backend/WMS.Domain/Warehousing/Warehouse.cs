using WMS.Shared.Exceptions;

namespace WMS.Domain.Warehousing;

public class Warehouse
{
    public Guid Id { get; protected set; }
    public string Code { get; protected set; } = string.Empty;
    public string Name { get; protected set; } = string.Empty;
    public WarehouseType Type { get; protected set; }
    public WarehouseStatus Status { get; protected set; }
    public string? Address { get; protected set; }
    public Guid? ParentWarehouseId { get; protected set; }
    public DateTime CreatedAt { get; protected set; }

    private readonly List<WarehouseLocation> _locations = [];
    public IReadOnlyCollection<WarehouseLocation> Locations => _locations.AsReadOnly();

    protected Warehouse() { }

    public static Warehouse Create(
        Guid id,
        string code,
        string name,
        WarehouseType type,
        string? address = null,
        Guid? parentWarehouseId = null)
    {
        if (type == WarehouseType.Machine)
            throw new ArgumentException("Makine depoları için MachineWarehouse.Create() kullanın.", nameof(type));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Depo kodu zorunludur.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Depo adı zorunludur.", nameof(name));
        if (code.Length > 20)
            throw new ArgumentException("Depo kodu en fazla 20 karakter olabilir.", nameof(code));
        if (name.Length > 200)
            throw new ArgumentException("Depo adı en fazla 200 karakter olabilir.", nameof(name));

        return new Warehouse
        {
            Id = id,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Type = type,
            Status = WarehouseStatus.Active,
            Address = address?.Trim(),
            ParentWarehouseId = parentWarehouseId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public WarehouseLocation CreateLocation(string code, string name, decimal? capacity = null)
    {
        if (Type == WarehouseType.Machine)
            throw new BusinessException("MACHINE_WAREHOUSE_NO_LOCATIONS", "Makine deposuna konum eklenemez.");
        if (Status != WarehouseStatus.Active)
            throw new BusinessException("WAREHOUSE_INACTIVE", "Devre dışı depoya konum eklenemez.");

        var location = WarehouseLocation.Create(Id, code, name, capacity);
        _locations.Add(location);
        return location;
    }

    public void Update(string name, string? address = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Depo adı zorunludur.", nameof(name));
        if (name.Length > 200)
            throw new ArgumentException("Depo adı en fazla 200 karakter olabilir.", nameof(name));

        Name = name.Trim();
        Address = address?.Trim();
    }

    public void Activate() => Status = WarehouseStatus.Active;

    public void Deactivate()
    {
        if (_locations.Any(l => l.IsActive))
            throw new BusinessException(
                "WAREHOUSE_HAS_ACTIVE_LOCATIONS", "Aktif konumları olan depo devre dışı bırakılamaz.");
        Status = WarehouseStatus.Inactive;
    }
}
