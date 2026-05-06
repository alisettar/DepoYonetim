using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

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
