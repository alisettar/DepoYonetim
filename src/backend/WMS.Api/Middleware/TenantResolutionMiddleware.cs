using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using WMS.Infrastructure.Services;
using WMS.Shared.Common;

namespace WMS.Api.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ICachedTenantConnectionFactory connFactory)
    {
        // Skip for auth endpoints and health checks
        var path = context.Request.Path.Value ?? "";
        if (path.StartsWith("/health") ||
            path.StartsWith("/api/v1/auth") ||
            path.StartsWith("/swagger") ||
            path.StartsWith("/favicon"))
        {
            await _next(context);
            return;
        }

        var authHeader = context.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Missing or invalid authorization header." });
            return;
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();
        try
        {
            var principal = ValidateJwtToken(token);
            if (principal is null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Invalid token." });
                return;
            }

            var tenantId = Guid.Parse(principal.FindFirst("tenant_id")?.Value ?? Guid.Empty.ToString());
            var tenantCode = principal.FindFirst("tenant_code")?.Value ?? "";
            var userId = Guid.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var email = principal.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var actorType = principal.FindFirst("actor_type")?.Value ?? "tenant_user";

            // Check tenant status
            var connStr = connFactory.GetConnectionString(tenantId);
            if (connStr is null)
            {
                context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
                await context.Response.WriteAsJsonAsync(new { message = "Tenant is not active." });
                return;
            }

            var tenantContext = TenantContext.FromClaims(tenantId, tenantCode, userId, email, actorType);
            context.Items["TenantContext"] = tenantContext;

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Tenant resolution failed");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Token validation failed." });
        }
    }

    private static ClaimsPrincipal? ValidateJwtToken(string token)
    {
        // Placeholder — will be replaced with real JWT validation using JwtTokenService
        try
        {
            // Basic structure validation only for now
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
    {
        return app.UseMiddleware<TenantResolutionMiddleware>();
    }
}

