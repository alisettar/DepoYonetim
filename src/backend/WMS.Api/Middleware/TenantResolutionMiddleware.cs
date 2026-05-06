using System.Security.Claims;
using WMS.Infrastructure.Services;
using WMS.Shared.Common;

namespace WMS.Api.Middleware;

public class TenantResolutionMiddleware(
    RequestDelegate next,
    ILogger<TenantResolutionMiddleware> logger,
    IConfiguration configuration)
{
    private static readonly string[] SkippedPaths = ["/health", "/api/v1/auth", "/swagger", "/favicon"];

    public async Task InvokeAsync(HttpContext context, ICachedTenantConnectionFactory connFactory)
    {
        var path = context.Request.Path.Value ?? "";
        if (SkippedPaths.Any(path.StartsWith))
        {
            await next(context);
            return;
        }

        var authHeader = context.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            // Auth devre dışı: varsayılan tenant kullan
            var defaultTenantId = configuration["DefaultTenantId"];
            if (defaultTenantId is null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Kimlik doğrulama gerekli." });
                return;
            }

            var tenantId = Guid.Parse(defaultTenantId);
            var connStr = connFactory.GetConnectionString(tenantId);
            if (connStr is null)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsJsonAsync(new { message = "Varsayılan tenant bağlantısı kurulamadı." });
                return;
            }

            context.Items["TenantContext"] = TenantContext.FromClaims(
                tenantId, "dev", Guid.Empty, "dev@localhost", "tenant_user");
            await next(context);
            return;
        }

        var token = authHeader["Bearer ".Length..].Trim();
        try
        {
            var principal = ValidateJwtToken(token);
            if (principal is null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Geçersiz token." });
                return;
            }

            var tenantId = Guid.Parse(principal.FindFirst("tenant_id")?.Value ?? Guid.Empty.ToString());
            var tenantCode = principal.FindFirst("tenant_code")?.Value ?? "";
            var userId = Guid.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var email = principal.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var actorType = principal.FindFirst("actor_type")?.Value ?? "tenant_user";

            var connStr = connFactory.GetConnectionString(tenantId);
            if (connStr is null)
            {
                context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
                await context.Response.WriteAsJsonAsync(new { message = "Tenant aktif değil." });
                return;
            }

            context.Items["TenantContext"] = TenantContext.FromClaims(tenantId, tenantCode, userId, email, actorType);
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Tenant çözümleme başarısız");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Token doğrulama başarısız." });
        }
    }

    private static ClaimsPrincipal? ValidateJwtToken(string token)
    {
        // Gerçek JWT doğrulama auth etkinleştirildiğinde JwtTokenService ile yapılacak
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return null;
            return new ClaimsPrincipal();
        }
        catch
        {
            return null;
        }
    }
}

public static class TenantResolutionMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app)
        => app.UseMiddleware<TenantResolutionMiddleware>();
}
