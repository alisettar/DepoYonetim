using MediatR;
using Microsoft.Extensions.Configuration;
using WMS.Shared.Common.Cryptography;
using WMS.Shared.Result;

namespace WMS.Application.Identity.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<RefreshTokenResponse>>;

public record RefreshTokenResponse(string AccessToken, DateTime ExpiresAt);

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly JwtTokenService _tokenService;
    private readonly IConfiguration _configuration;

    public RefreshTokenHandler(JwtTokenService tokenService, IConfiguration configuration)
    {
        _tokenService = tokenService;
        _configuration = configuration;
    }

    public Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        // TODO: validate refresh token against DB, rotate if rotated
        // For now, return a placeholder that will fail at runtime until fully implemented
        return Result<RefreshTokenResponse>.Failure<RefreshTokenResponse>("NOT_IMPLEMENTED", "Refresh token validation against DB is not yet implemented. Use the Login endpoint first").ToTask();
    }
}
