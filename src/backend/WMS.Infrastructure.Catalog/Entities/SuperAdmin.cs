namespace WMS.Infrastructure.Catalog.Entities;

public class SuperAdmin
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? MfaSecret { get; set; }
    public bool IsLocked { get; set; }
    public DateTime CreatedAt { get; set; }
}
