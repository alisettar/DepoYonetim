using Microsoft.Extensions.Caching.Memory;

namespace WMS.Infrastructure.Services;

public interface ICachedTenantConnectionFactory
{
    string? GetConnectionString(Guid tenantId);
    Task<string> GetConnectionStringAsync(Guid tenantId, CancellationToken ct = default);
}

public class CachedTenantConnectionFactory(
    ITenantConnectionResolver resolver,
    IMemoryCache cache) : ICachedTenantConnectionFactory
{
    private static readonly TimeSpan TTL = TimeSpan.FromMinutes(10);

    public string? GetConnectionString(Guid tenantId)
    {
        var key = $"connstr:{tenantId}";
        if (cache.TryGetValue(key, out string? connStr))
            return connStr;

        var result = resolver.GetConnectionStringAsync(tenantId).GetAwaiter().GetResult();
        if (result is not null)
        {
            cache.Set(key, result, TTL);
            return result;
        }
#pragma warning disable CS8603 // Possible null reference return
        return null;
#pragma warning restore CS8603
    }

    public async Task<string> GetConnectionStringAsync(Guid tenantId, CancellationToken ct = default)
    {
        var key = $"connstr:{tenantId}";
        if (cache.TryGetValue(key, out string? connStr))
            return connStr;

        var result = await resolver.GetConnectionStringAsync(tenantId, ct);
        if (result is not null)
            cache.Set(key, result, TTL);

        return result ?? string.Empty;
    }
}
