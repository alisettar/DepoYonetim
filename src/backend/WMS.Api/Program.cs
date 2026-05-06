using Microsoft.EntityFrameworkCore;
using WMS.Infrastructure.Persistence;
using WMS.Infrastructure.Services;
using WMS.Api.Endpoints;
using WMS.Domain.Warehousing;

var builder = WebApplication.CreateBuilder(args);

// Docker'da 0.0.0.0'a dinle (launchSettings.json override'ını bypass et)
builder.WebHost.UseUrls("http://0.0.0.0:5000");

// Prototip: direkt connection string ile DbContext
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Host=localhost;Port=5432;Database=wms_prototype;";

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// Prototip DbContext
builder.Services.AddScoped<PrototypeDbContext>(sp =>
    new PrototypeDbContext(connectionString));

// Warehouse prototip servisi
builder.Services.AddScoped<WarehouseService>();

// Warehouse endpoint'lerini kaydet
var builderWithWarehouses = builder;

var app = builderWithWarehouses.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // İlk çalıştırmada tablo oluştur (prototip)
    using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<PrototypeDbContext>();
    db.Database.EnsureCreated();

    // Seed data — örnek depo + konumlar
    SeedData(db);
}

// Health checks
app.MapGet("/", () => new { name = "WMS Prototype", version = "1.0.0" });
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

// Warehouse endpoints
app.MapWarehousesEndpoints();

app.Run();

static void SeedData(PrototypeDbContext db)
{
    if (!db.Warehouses.Any())
    {
        var w1 = Warehouse.Create(Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), "ANA-DEPO", "Ana Depo", Guid.Empty, "İstanbul");
        w1.CreateLocation("A", "01", "01", "01");
        w1.CreateLocation("A", "01", "01", "02");
        w1.CreateLocation("A", "01", "02", "01");
        w1.CreateLocation("B", "02", "01", "01");
        w1.CreateLocation("B", "02", "02", "01");
        db.Warehouses.Add(w1);

        var w2 = Warehouse.Create(Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"), "YAN-DEPO", "Yan Depo", Guid.Empty, "Ankara");
        w2.CreateLocation("C", "01", "01", "01");
        w2.CreateLocation("C", "01", "02", "01");
        db.Warehouses.Add(w2);

        db.SaveChanges();
    }
}
