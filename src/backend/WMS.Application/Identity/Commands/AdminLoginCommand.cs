using MediatR;
using WMS.Domain.Identity.Services;
using WMS.Shared.Common.Cryptography;
using WMS.Shared.Result;

namespace WMS.Application.Identity.Commands;

public record AdminLoginCommand(string Email, string Password) : IRequest<Result<AdminLoginResponse>>;

public record AdminLoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    AdminUserInfo User);

public record AdminUserInfo(
    Guid UserId,
    string Email,
    string ActorType);

public class AdminLoginHandler(
    IAuthenticationService authService,
    JwtTokenService tokenService) : IRequestHandler<AdminLoginCommand, Result<AdminLoginResponse>>
{
    public async Task<Result<AdminLoginResponse>> Handle(AdminLoginCommand request, CancellationToken ct)
    {
        var loginResult = await authService.AuthenticateAsync(request.Email, request.Password, true, ct);
        if (loginResult.IsFailure)
            return Result<AdminLoginResponse>.Failure<AdminLoginResponse>(loginResult.ErrorCode, loginResult.Message);

        var tokenResult = tokenService.GenerateToken(
            loginResult.Value.UserId,
            loginResult.Value.Email,
            Guid.Empty,
            string.Empty,
            "super_admin",
            ["catalog.*", "admin.tenant.*", "admin.user.*"]);

        return Result<AdminLoginResponse>.Success(new AdminLoginResponse(
            tokenResult.AccessToken,
            tokenResult.RefreshToken,
            tokenResult.ExpiresAt,
            new AdminUserInfo(loginResult.Value.UserId, loginResult.Value.Email, "super_admin")));
    }
}
