namespace WMS.Domain.Catalog;

public class Tenant
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Status { get; private set; } = "Active";
    public string? Plan { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Tenant Create(
        Guid id,
        string code,
        string name,
        string? plan = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Tenant code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tenant name is required.", nameof(name));

        return new Tenant
        {
            Id = id,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Status = "Active",
            Plan = plan,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Suspend()
    {
        if (Status == "Deleted")
            throw new WMS.Shared.Exceptions.BusinessException("Cannot suspend a deleted tenant.", "TENANT_DELETED");

        Status = "Suspended";
    }

    public void Activate()
    {
        if (Status == "Deleted")
            throw new WMS.Shared.Exceptions.BusinessException("Cannot activate a deleted tenant.", "TENANT_DELETED");

        Status = "Active";
    }

    public void Delete() => Status = "Deleted";
}
