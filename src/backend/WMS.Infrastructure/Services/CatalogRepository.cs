using Microsoft.EntityFrameworkCore;
using WMS.Domain.Catalog.Services;
using WMS.Domain.Identity;
using WMS.Infrastructure.Persistence;
using WMS.Shared.Result;

namespace WMS.Infrastructure.Services;

public class CatalogRepository(AppDbContext dbContext) : ICatalogRepository
{
    public async Task<TenantUser?> GetUserByEmailAsync(string email, CancellationToken ct = default)
    {
        return await dbContext.TenantUsers
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant().Trim(), ct);
    }

    public async Task<Result<TenantUser>> CreateUserAsync(
        Guid id,
        Guid tenantId,
        string email,
        string passwordHash,
        string fullName,
        string roleCode,
        CancellationToken ct = default)
    {
        var existing = await dbContext.TenantUsers
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant().Trim(), ct);

        if (existing is not null)
            return Result<TenantUser>.MakeFailure("USER_ALREADY_EXISTS", "A user with this email already exists.");

        var user = TenantUser.Create(id, tenantId, email, passwordHash, fullName, roleCode);
        dbContext.TenantUsers.Add(user);
        await dbContext.SaveChangesAsync(ct);

        return Result<TenantUser>.MakeSuccess(user);
    }

    public async Task<Result> DeleteUserAsync(string email, CancellationToken ct = default)
    {
        var user = await dbContext.TenantUsers
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant().Trim(), ct);

        if (user is null)
            return WMS.Shared.Result.Result.Failure("NOT_FOUND", "User not found.");

        dbContext.TenantUsers.Remove(user);
        await dbContext.SaveChangesAsync(ct);

        return WMS.Shared.Result.Result.Success();
    }

    public async Task<WMS.Domain.Identity.SuperAdmin?> GetSuperAdminByEmailAsync(string email, CancellationToken ct = default)
    {
        return await dbContext.SuperAdmins
            .FirstOrDefaultAsync(a => a.Email == email.ToLowerInvariant().Trim(), ct);
    }
}
