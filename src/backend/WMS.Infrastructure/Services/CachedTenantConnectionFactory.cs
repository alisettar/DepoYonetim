using Microsoft.Extensions.Caching.Memory;

namespace WMS.Infrastructure.Services;

public interface ICachedTenantConnectionFactory
{
    string? GetConnectionString(Guid tenantId);
    Task<string> GetConnectionStringAsync(Guid tenantId, CancellationToken ct = default);
}

public class CachedTenantConnectionFactory : ICachedTenantConnectionFactory
{
    private readonly ITenantConnectionResolver _resolver;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan TTL = TimeSpan.FromMinutes(10);

    public CachedTenantConnectionFactory(
        ITenantConnectionResolver resolver,
        IMemoryCache cache)
    {
        _resolver = resolver;
        _cache = cache;
    }

    public string? GetConnectionString(Guid tenantId)
    {
        var key = $"connstr:{tenantId}";
        if (_cache.TryGetValue(key, out string? connStr))
            return connStr;

        var result = _resolver.GetConnectionStringAsync(tenantId).GetAwaiter().GetResult();
        if (result is not null)
        {
            _cache.Set(key, result, TTL);
            return result;
        }
#pragma warning disable CS8603 // Possible null reference return
        return null;
#pragma warning restore CS8603
    }

    public async Task<string> GetConnectionStringAsync(Guid tenantId, CancellationToken ct = default)
    {
        var key = $"connstr:{tenantId}";
        if (_cache.TryGetValue(key, out string? connStr))
            return connStr;

        var result = await _resolver.GetConnectionStringAsync(tenantId, ct);
        if (result is not null)
            _cache.Set(key, result, TTL);

        return result ?? string.Empty;
    }
}
