using MediatR;
using WMS.Domain.Identity.Services;
using WMS.Domain.Catalog.Services;
using WMS.Infrastructure.Persistence;
using WMS.Infrastructure.Services;
using WMS.Shared.Common;
using WMS.Shared.Common.Cryptography;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// Infrastructure - scoped services
builder.Services.AddScoped<ITenantContext>(ctx =>
    ctx.GetService<TenantContext>() ?? throw new InvalidOperationException("TenantContext not registered."));

builder.Services.AddScoped<AppDbContext>();
builder.Services.AddScoped<IAuthenticationService, AuthService>();
builder.Services.AddScoped<ICatalogRepository, CatalogRepository>();
builder.Services.AddScoped<JwtTokenService>();

// MediatR - register all handlers from WMS.Application assembly
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(WMS.Application.Identity.Commands.LoginHandler).Assembly));

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseSwagger();

// Health checks
app.MapGet("/", () => new { name = "WMS", version = "1.0.0" });
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.Run();
