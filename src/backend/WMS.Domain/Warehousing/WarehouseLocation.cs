namespace WMS.Domain.Warehousing;

public class WarehouseLocation
{
    public Guid Id { get; private set; }
    public Guid WarehouseId { get; private set; }
    public string Zone { get; private set; } = string.Empty;
    public string Aisle { get; private set; } = string.Empty;
    public string Section { get; private set; } = string.Empty;
    public string Bin { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public string FullName => $"{Zone}-{Aisle}-{Section}-{Bin}";

    private WarehouseLocation() { }

    public static WarehouseLocation Create(
        Guid warehouseId,
        string zone,
        string aisle,
        string section,
        string bin)
    {
        if (string.IsNullOrWhiteSpace(zone))
            throw new ArgumentException("Zone is required.", nameof(zone));
        if (string.IsNullOrWhiteSpace(aisle))
            throw new ArgumentException("Aisle is required.", nameof(aisle));
        if (string.IsNullOrWhiteSpace(section))
            throw new ArgumentException("Section is required.", nameof(section));
        if (string.IsNullOrWhiteSpace(bin))
            throw new ArgumentException("Bin is required.", nameof(bin));

        return new WarehouseLocation
        {
            Id = Guid.NewGuid(),
            WarehouseId = warehouseId,
            Zone = zone.Trim().ToUpperInvariant(),
            Aisle = aisle.Trim().ToUpperInvariant(),
            Section = section.Trim().ToUpperInvariant(),
            Bin = bin.Trim().ToUpperInvariant(),
            IsActive = true
        };
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
