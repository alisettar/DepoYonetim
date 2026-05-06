using WMS.Shared.Result;

namespace WMS.Domain.Identity.Services;

public record LoginResult(
    Guid UserId,
    string Email,
    Guid TenantId,
    string TenantCode,
    string RoleCode,
    string[] Permissions);

public interface IAuthenticationService
{
    Task<Result<LoginResult>> AuthenticateAsync(
        string email,
        string password,
        bool isSuperAdmin = false,
        CancellationToken ct = default);
}
