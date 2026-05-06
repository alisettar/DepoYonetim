using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using WMS.Infrastructure.Catalog;
using WMS.Infrastructure.Catalog.Entities;
using WMS.Shared.Common.Cryptography;

namespace WMS.Infrastructure.Services;

public interface ITenantConnectionResolver
{
    Task<string?> GetConnectionStringAsync(Guid tenantId, CancellationToken ct = default);
    Task<string> GetConnectionStringAsync(string tenantCode, CancellationToken ct = default);
}

public class TenantConnectionResolver : ITenantConnectionResolver
{
    private readonly Func<CatalogDbContext> _catalogContextFactory;
    private readonly AesGcmService _decryptor;

    public TenantConnectionResolver(
        Func<CatalogDbContext> catalogContextFactory,
        AesGcmService decryptor)
    {
        _catalogContextFactory = catalogContextFactory;
        _decryptor = decryptor;
    }

    public async Task<string?> GetConnectionStringAsync(Guid tenantId, CancellationToken ct = default)
    {
        using var context = _catalogContextFactory();
        var db = await context.TenantDatabases
            .Include(t => t.Tenant)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId, ct);

        if (db is null || db.Tenant?.Status != "Active")
            return null;

        return BuildConnectionString(db);
    }

    public async Task<string> GetConnectionStringAsync(string tenantCode, CancellationToken ct = default)
    {
        using var context = _catalogContextFactory();
        var tenant = await context.Tenants
            .Include(t => t.TenantDatabases)
            .FirstOrDefaultAsync(t => t.Code == tenantCode, ct);

        if (tenant is null || tenant.Status != "Active" || !tenant.TenantDatabases.Any())
            throw new KeyNotFoundException($"Tenant '{tenantCode}' not found or not active.");

        var db = tenant.TenantDatabases.First();
        return BuildConnectionString(db);
    }

    private string BuildConnectionString(TenantDatabase db)
    {
        var password = _decryptor.Decrypt(Convert.ToHexString(db.PasswordEnc));
        return $"Host={db.Host};Port={db.Port};Database={db.DatabaseName};Username={db.Username};Password={password};";
    }
}
