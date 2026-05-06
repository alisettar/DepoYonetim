using WMS.Domain.Warehousing;
using WMS.Infrastructure.Services;

namespace WMS.Api.Endpoints;

public static class WarehousesEndpoints
{
    public static WebApplication MapWarehousesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/warehouses").WithTags("Warehouses");

        group.MapGet("/", async (WarehouseService service) =>
        {
            var warehouses = await service.GetAllAsync();
            return warehouses.Select(w => new {
                w.Id, w.Code, w.Name, w.Address,
                w.IsActive, w.CreatedAt,
                locations = w.Locations.Select(l => new {
                    l.Id, l.Zone, l.Aisle, l.Section, l.Bin, l.FullName, l.IsActive
                })
            });
        }).WithName("ListWarehouses");

        group.MapGet("/{id:guid}", async (Guid id, WarehouseService service) =>
        {
            var warehouse = await service.GetByIdAsync(id);
            return warehouse is not null ? (IResult)Results.Ok(warehouse) : Results.NotFound();
        }).WithName("GetWarehouse");

        group.MapPost("/", async (CreateWarehouseRequest request, WarehouseService service) =>
        {
            var warehouse = await service.CreateAsync(request.Code, request.Name, request.Address);
            await service.SaveAsync();
            return Results.Created($"/api/v1/warehouses/{warehouse.Id}", warehouse);
        }).WithName("CreateWarehouse");

        group.MapPut("/{id:guid}", async (Guid id, UpdateWarehouseRequest request, WarehouseService service) =>
        {
            try
            {
                var result = await service.UpdateAsync(id, request.Name, request.Address);
                await service.SaveAsync();
                return Results.Ok(result.Warehouse);
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        }).WithName("UpdateWarehouse");

        group.MapDelete("/{id:guid}", async (Guid id, WarehouseService service) =>
        {
            await service.DeleteAsync(id);
            await service.SaveAsync();
            return Results.NoContent();
        }).WithName("DeleteWarehouse");

        return app;
    }

    public record CreateWarehouseRequest(string Code, string Name, string? Address = null);
    public record UpdateWarehouseRequest(string Name, string? Address = null);
}
