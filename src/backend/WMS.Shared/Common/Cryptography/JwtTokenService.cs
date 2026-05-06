using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace WMS.Shared.Common.Cryptography;

public record TokenResult(string AccessToken, string RefreshToken, DateTime ExpiresAt);

public class JwtTokenService(string privateKeyPath, string publicKeyPath, string issuer)
{
    private readonly string _privateKeyPem = File.ReadAllText(privateKeyPath);
    private readonly string _publicKeyPem = File.ReadAllText(publicKeyPath);

    public TokenResult GenerateToken(
        Guid userId,
        string email,
        Guid tenantId,
        string tenantCode,
        string actorType,
        string[] permissions,
        TimeSpan? accessTokenLifetime = null)
    {
        accessTokenLifetime ??= TimeSpan.FromMinutes(15);
        var now = DateTime.UtcNow;
        var expiration = now.Add(accessTokenLifetime.Value);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new("tenant_id", tenantId.ToString()),
            new("tenant_code", tenantCode),
            new("actor_type", actorType),
            new(JwtRegisteredClaimNames.Iss, issuer),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString()),
            new(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(now).ToUnixTimeSeconds().ToString()),
            new(JwtRegisteredClaimNames.Exp, new DateTimeOffset(expiration).ToUnixTimeSeconds().ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(permissions.Select(p => new Claim("perms", p)));

        var key = new RsaSecurityKey(ParsePrivatePem(_privateKeyPem));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            claims: claims,
            notBefore: now,
            expires: expiration,
            signingCredentials: credentials);

        var handler = new JwtSecurityTokenHandler();
        var accessToken = handler.WriteToken(token);

        var refreshToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

        return new TokenResult(accessToken, refreshToken, expiration);
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var validation = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(ParsePublicPem(_publicKeyPem)),
            ClockSkew = TimeSpan.FromSeconds(0)
        };

        return handler.ValidateToken(token, validation, out _);
    }

    private static RSA ParsePrivatePem(string pem)
    {
        var lines = pem
            .Replace("-----BEGIN PRIVATE KEY-----", "")
            .Replace("-----END PRIVATE KEY-----", "")
            .Replace("\n", "")
            .Replace("\r", "")
            .Replace(" ", "")
            .Split('\n')
            .Where(l => !l.StartsWith("-----"))
            .Aggregate(string.Empty, (a, b) => a + b);

        var rsa = RSA.Create();
        var keyBytes = Convert.FromHexString(lines);
        rsa.ImportPkcs8PrivateKey(keyBytes, out _);
        return rsa;
    }

    private static RSA ParsePublicPem(string pem)
    {
        var lines = pem
            .Replace("-----BEGIN PUBLIC KEY-----", "")
            .Replace("-----END PUBLIC KEY-----", "")
            .Replace("\n", "")
            .Replace("\r", "")
            .Replace(" ", "")
            .Split('\n')
            .Where(l => !l.StartsWith("-----"))
            .Aggregate(string.Empty, (a, b) => a + b);

        var rsa = RSA.Create();
        var keyBytes = Convert.FromHexString(lines);
        rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);
        return rsa;
    }

    public string ExportPublicKeyPem()
    {
        return _publicKeyPem;
    }
}

public static class DateTimeExtensions
{
    public static long ToEpochSeconds(this DateTimeOffset dto) => dto.ToUnixTimeSeconds();
}
