namespace WMS.Domain.Identity;

public class SuperAdmin
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool IsLocked { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public string? MfaSecret { get; private set; }

    public static SuperAdmin Create(Guid id, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        return new SuperAdmin
        {
            Id = id,
            Email = email.ToLowerInvariant().Trim(),
            PasswordHash = passwordHash,
            IsLocked = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public bool VerifyPassword(string password)
    {
        return WMS.Shared.Common.Cryptography.PasswordHasher.Verify(password, PasswordHash);
    }

    public void Lock() => IsLocked = true;
    public void Unlock() => IsLocked = false;

    public void SetLastLoginAt() => LastLoginAt = DateTime.UtcNow;
}
