namespace WMS.Infrastructure.Catalog.Entities;

public class TenantDatabase
{
    public Guid TenantId { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 5432;
    public string DatabaseName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public byte[] PasswordEnc { get; set; } = [];
    public string? Region { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}
