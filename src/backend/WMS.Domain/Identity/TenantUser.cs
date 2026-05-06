namespace WMS.Domain.Identity;

public class TenantUser
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string RoleCode { get; private set; } = "Viewer";
    public bool IsActive { get; private set; } = true;
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static TenantUser Create(
        Guid id,
        Guid tenantId,
        string email,
        string passwordHash,
        string fullName,
        string roleCode)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required.", nameof(fullName));
        if (string.IsNullOrWhiteSpace(roleCode))
            throw new ArgumentException("Role code is required.", nameof(roleCode));

        var roleValid = roleCode is "SuperAdmin" or "TenantAdmin" or "WarehouseManager" or "Operator" or "Viewer";
        if (!roleValid)
            throw new ArgumentException($"Invalid role code: {roleCode}. Valid values: SuperAdmin, TenantAdmin, WarehouseManager, Operator, Viewer", nameof(roleCode));

        return new TenantUser
        {
            Id = id,
            TenantId = tenantId,
            Email = email.ToLowerInvariant().Trim(),
            PasswordHash = passwordHash,
            FullName = fullName.Trim(),
            RoleCode = roleCode,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void RecordLogin()
    {
        FailedLoginAttempts = 0;
        LastLoginAt = DateTime.UtcNow;
    }

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= 5)
            IsActive = false;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
