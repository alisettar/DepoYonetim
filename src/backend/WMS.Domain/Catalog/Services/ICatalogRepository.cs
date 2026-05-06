using WMS.Domain.Identity;
using WMS.Shared.Result;

namespace WMS.Domain.Catalog.Services;

public interface ICatalogRepository
{
    Task<TenantUser?> GetUserByEmailAsync(string email, CancellationToken ct = default);
    Task<Result<TenantUser>> CreateUserAsync(
        Guid id,
        Guid tenantId,
        string email,
        string passwordHash,
        string fullName,
        string roleCode,
        CancellationToken ct = default);
    Task<Result> DeleteUserAsync(string email, CancellationToken ct = default);
    Task<WMS.Domain.Identity.SuperAdmin?> GetSuperAdminByEmailAsync(string email, CancellationToken ct = default);
}
