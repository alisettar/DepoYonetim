using Microsoft.EntityFrameworkCore;
using WMS.Api.Endpoints;
using WMS.Api.Middleware;
using WMS.Application;
using WMS.Application.Catalog;
using WMS.Application.Inventory;
using WMS.Application.Warehousing;
using WMS.Infrastructure.Persistence;
using WMS.Infrastructure.Persistence.Seeds;
using WMS.Infrastructure.Repositories;
using WMS.Infrastructure.Services;
using WMS.Shared.Common;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5000");

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

builder.Services.AddSingleton<ICachedTenantConnectionFactory>(
    _ => new DevTenantConnectionFactory(defaultTenantId, defaultConnStr));

builder.Services.AddScoped<ITenantContext>(sp =>
{
    var accessor = sp.GetRequiredService<IHttpContextAccessor>();
    return accessor.HttpContext?.Items["TenantContext"] as ITenantContext
        ?? new TenantContext { TenantId = defaultTenantId };
});

builder.Services.AddScoped<AppDbContext>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ILotRepository, LotRepository>();
builder.Services.AddScoped<IStockMovementRepository, StockMovementRepository>();
builder.Services.AddScoped<IStockBalanceRepository, StockBalanceRepository>();
builder.Services.AddScoped<IFifoLayerRepository, FifoLayerRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    var opts = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(defaultConnStr).Options;
    using var db = new AppDbContext(opts);
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();
    DatabaseSeeder.Seed(db);
}

app.MapGet("/", () => new { name = "WMS API", version = "1.0.0" });
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.UseTenantResolution();
app.MapWarehousesEndpoints();
app.MapUnitsEndpoints();
app.MapCategoriesEndpoints();
app.MapProductsEndpoints();
app.MapInventoryEndpoints();

app.Run();

internal sealed class DevTenantConnectionFactory(Guid tenantId, string connectionString)
    : ICachedTenantConnectionFactory
{
    public string? GetConnectionString(Guid id)
        => id == tenantId ? connectionString : null;

    public Task<string> GetConnectionStringAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(id == tenantId ? connectionString : string.Empty);
}
