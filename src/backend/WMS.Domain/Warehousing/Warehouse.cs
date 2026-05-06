namespace WMS.Domain.Warehousing;

public class Warehouse
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Guid TenantId { get; private set; }
    public string? Address { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<WarehouseLocation> _locations = [];
    public IReadOnlyCollection<WarehouseLocation> Locations => _locations.AsReadOnly();

    private Warehouse() { }

    public static Warehouse Create(
        Guid id,
        string code,
        string name,
        Guid tenantId,
        string? address = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Warehouse code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Warehouse name is required.", nameof(name));
        if (code.Length > 20)
            throw new ArgumentException("Warehouse code max length is 20.", nameof(code));
        if (name.Length > 200)
            throw new ArgumentException("Warehouse name max length is 200.", nameof(name));

        return new Warehouse
        {
            Id = id,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            TenantId = tenantId,
            Address = address?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public WarehouseLocation CreateLocation(
        string zone,
        string aisle,
        string section,
        string bin)
    {
        if (!IsActive)
            throw new WMS.Shared.Exceptions.BusinessException(
                "Cannot create location in an inactive warehouse.", "WAREHOUSE_INACTIVE");

        var location = WarehouseLocation.Create(Id, zone, aisle, section, bin);
        _locations.Add(location);
        return location;
    }

    public void Update(string name, string? address = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Warehouse name is required.", nameof(name));
        if (name.Length > 200)
            throw new ArgumentException("Warehouse name max length is 200.", nameof(name));

        Name = name.Trim();
        Address = address?.Trim();
    }

    public void Deactivate()
    {
        if (!_locations.Any())
            throw new WMS.Shared.Exceptions.BusinessException(
                "Warehouse must have no locations to deactivate.", "WAREHOUSE_HAS_LOCATIONS");

        IsActive = false;
    }
}
