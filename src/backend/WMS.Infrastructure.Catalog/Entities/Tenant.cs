namespace WMS.Infrastructure.Catalog.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Active"; // Active | Suspended | Failed | Deleted
    public string? Plan { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual ICollection<TenantDatabase> TenantDatabases { get; set; } = [];
}
