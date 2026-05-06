using MediatR;
using WMS.Domain.Identity;
using WMS.Domain.Identity.Services;
using WMS.Shared.Common.Cryptography;
using WMS.Shared.Result;

namespace WMS.Application.Identity.Commands;

public record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserInfo User);

public record UserInfo(
    Guid UserId,
    string Email,
    Guid TenantId,
    string TenantCode,
    string RoleCode,
    string[] Permissions,
    string ActorType);

public class LoginHandler(
    IAuthenticationService authService,
    JwtTokenService tokenService) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var loginResult = await authService.AuthenticateAsync(request.Email, request.Password, false, ct);
        if (loginResult.IsFailure)
            return Result<LoginResponse>.Failure<LoginResponse>(loginResult.ErrorCode, loginResult.Message);

        var permissions = GetPermissionsForRole(loginResult.Value.RoleCode);
        var tokenResult = tokenService.GenerateToken(
            loginResult.Value.UserId,
            loginResult.Value.Email,
            loginResult.Value.TenantId,
            loginResult.Value.TenantCode,
            "tenant_user",
            permissions);

        return Result<LoginResponse>.Success(new LoginResponse(
            tokenResult.AccessToken,
            tokenResult.RefreshToken,
            tokenResult.ExpiresAt,
            new UserInfo(
                loginResult.Value.UserId,
                loginResult.Value.Email,
                loginResult.Value.TenantId,
                loginResult.Value.TenantCode,
                loginResult.Value.RoleCode,
                permissions,
                "tenant_user")));
    }

    private static string[] GetPermissionsForRole(string roleCode)
        => roleCode switch
        {
            "SuperAdmin" => [
                "catalog.*", "admin.tenant.*", "admin.user.*",
                "inventory.*", "warehousing.*", "production.*",
                "recipes.*", "dashboard.*"],
            "TenantAdmin" => [
                "inventory.*", "warehousing.*", "products.*",
                "recipes.*", "production.*", "dashboard.*",
                "tenant.*", "users.*", "roles.*"],
            "WarehouseManager" => [
                "inventory.read", "inventory.write",
                "warehousing.*", "products.read",
                "recipes.read", "dashboard.*"],
            "Operator" => [
                "machines.load", "machines.unload",
                "production-orders.start", "production-orders.output",
                "balances.read"],
            "Viewer" => [
                "inventory.read", "warehousing.read",
                "products.read", "recipes.read",
                "production-orders.read", "dashboard.*"],
            _ => ["inventory.read", "dashboard.*"]
        };
}
