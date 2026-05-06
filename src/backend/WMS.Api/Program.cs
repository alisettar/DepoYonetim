using Microsoft.EntityFrameworkCore;
using WMS.Api.Endpoints;
using WMS.Api.Middleware;
using WMS.Application;
using WMS.Application.Warehousing;
using WMS.Domain.Warehousing;
using WMS.Infrastructure.Persistence;
using WMS.Infrastructure.Repositories;
using WMS.Infrastructure.Services;
using WMS.Shared.Common;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5000");

// Auth henüz aktif değil; kayıtlı olmayan handler bağımlılıkları startup'ta hata vermez
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateOnBuild = false;
});

var defaultTenantId = Guid.Parse(
    builder.Configuration["DefaultTenantId"] ?? "a0000000-0000-0000-0000-000000000001");
var defaultConnStr = builder.Configuration.GetConnectionString("Default")
    ?? "Host=localhost;Port=5432;Database=wms_dev;Username=postgres;Password=postgres;";

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<ApplicationAssemblyMarker>());

// Dev ortamı: CatalogDB lookup yerine sabit bağlantı döner
builder.Services.AddSingleton<ICachedTenantConnectionFactory>(
    _ => new DevTenantConnectionFactory(defaultTenantId, defaultConnStr));

// ITenantContext: HttpContext.Items["TenantContext"] üzerinden çözümlenir
builder.Services.AddScoped<ITenantContext>(sp =>
{
    var accessor = sp.GetRequiredService<IHttpContextAccessor>();
    return accessor.HttpContext?.Items["TenantContext"] as ITenantContext
        ?? new TenantContext { TenantId = defaultTenantId };
});

builder.Services.AddScoped<AppDbContext>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    var opts = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(defaultConnStr).Options;
    using var db = new AppDbContext(opts);
    db.Database.EnsureCreated();
    SeedData(db);
}

app.MapGet("/", () => new { name = "WMS API", version = "1.0.0" });
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.UseTenantResolution();
app.MapWarehousesEndpoints();

app.Run();

static void SeedData(AppDbContext db)
{
    if (db.Warehouses.Any()) return;

    var w1 = Warehouse.Create(
        Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
        "ANA-DEPO", "Ana Depo", WarehouseType.Physical, "İstanbul");
    w1.CreateLocation("A-01", "Koridor A - Raf 1");
    w1.CreateLocation("A-02", "Koridor A - Raf 2");
    w1.CreateLocation("B-01", "Koridor B - Raf 1", capacity: 500);
    db.Warehouses.Add(w1);

    var w2 = Warehouse.Create(
        Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
        "KARANT", "Karantina Deposu", WarehouseType.Virtual);
    db.Warehouses.Add(w2);

    var m1 = MachineWarehouse.Create(
        Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012"),
        "MK-001", "Torna Makinesi Deposu", "TORNA-001", "Üretim Sahası");
    db.Warehouses.Add(m1);

    db.SaveChanges();
}

// Dev ortamı için: CatalogDB olmadan sabit bağlantı döner
internal sealed class DevTenantConnectionFactory(Guid tenantId, string connectionString)
    : ICachedTenantConnectionFactory
{
    public string? GetConnectionString(Guid id)
        => id == tenantId ? connectionString : null;

    public Task<string> GetConnectionStringAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(id == tenantId ? connectionString : string.Empty);
}
