using Microsoft.EntityFrameworkCore;
using WMS.Domain.Identity;
using WMS.Domain.Identity.Services;
using WMS.Infrastructure.Persistence;
using WMS.Shared.Common;
using WMS.Shared.Common.Cryptography;
using WMS.Shared.Result;

namespace WMS.Infrastructure.Services;

public class AuthService(AppDbContext dbContext, ITenantContext tenantContext) : IAuthenticationService
{
    public async Task<Result<LoginResult>> AuthenticateAsync(
        string email,
        string password,
        bool isSuperAdmin = false,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Fail("EMPTY_EMAIL", "Email is required.");

        if (string.IsNullOrWhiteSpace(password))
            return Fail("EMPTY_PASSWORD", "Password is required.");

        if (isSuperAdmin)
            return await AuthenticateSuperAdminAsync(email, password, ct);

        var user = await dbContext.TenantUsers
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant().Trim(), ct);

        if (user is null)
            return Fail("AUTH_FAILED", "Invalid email or password.");

        if (!user.IsActive)
            return Fail("AUTH_FAILED", "Account is deactivated.");

        if (VerifyPassword(password, user.PasswordHash))
        {
            user.RecordLogin();
            await dbContext.SaveChangesAsync(ct);

            var permissions = GetPermissionsForRole(user.RoleCode);

            return Ok(new LoginResult(
                user.Id,
                user.Email,
                tenantContext.TenantId,
                tenantContext.TenantCode,
                user.RoleCode,
                permissions));
        }

        user.RecordFailedLogin();
        await dbContext.SaveChangesAsync(ct);

        return Fail("AUTH_FAILED", "Invalid email or password.");
    }

    private async Task<Result<LoginResult>> AuthenticateSuperAdminAsync(
        string email,
        string password,
        CancellationToken ct)
    {
        var admin = await dbContext.SuperAdmins
            .FirstOrDefaultAsync(a => a.Email == email.ToLowerInvariant().Trim(), ct);

        if (admin is null || !admin.VerifyPassword(password))
            return Fail("AUTH_FAILED", "Invalid email or password.");

        if (admin.IsLocked)
            return Fail("AUTH_FAILED", "Account is locked.");

        admin.SetLastLoginAt();
        await dbContext.SaveChangesAsync(ct);

        return Ok(new LoginResult(
            admin.Id,
            admin.Email,
            Guid.Empty,
            "SUPER",
            "SuperAdmin",
            new[] { "all" }));
    }

    private static Result<LoginResult> Fail(string errorCode, string message)
        => Result<LoginResult>.MakeFailure(errorCode, message);

    private static Result<LoginResult> Ok(LoginResult value)
        => Result<LoginResult>.MakeSuccess(value);

    private static bool VerifyPassword(string password, string passwordHash)
        => PasswordHasher.Verify(password, passwordHash);

    private static string[] GetPermissionsForRole(string roleCode)
        => roleCode switch
        {
            "SuperAdmin" => new[] { "all" },
            "TenantAdmin" => new[] { "dashboard.read", "inventory.read", "inventory.write", "users.manage" },
            "WarehouseManager" => new[] { "dashboard.read", "inventory.read", "inventory.write", "movements.manage" },
            "Operator" => new[] { "dashboard.read", "inventory.read", "movements.create" },
            "Viewer" => new[] { "dashboard.read", "inventory.read" },
            _ => new string[0]
        };
}
