using MediatR;
using WMS.Application.Warehousing.Commands;
using WMS.Application.Warehousing.Queries;
using WMS.Domain.Warehousing;

namespace WMS.Api.Endpoints;

public static class WarehousesEndpoints
{
    public static WebApplication MapWarehousesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/warehouses").WithTags("Warehouses");

        group.MapGet("/", async (WarehouseType? type, ISender sender) =>
        {
            var result = await sender.Send(new ListWarehousesQuery(type));
            return Results.Ok(result);
        }).WithName("ListWarehouses");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetWarehouseQuery(id));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { result.ErrorCode, result.Message });
        }).WithName("GetWarehouse");

        group.MapPost("/", async (CreateWarehouseRequest req, ISender sender) =>
        {
            var result = await sender.Send(new CreateWarehouseCommand(
                req.Code, req.Name, req.Type, req.Address, req.ParentWarehouseId, req.MachineCode));

            return result.IsSuccess
                ? Results.Created($"/api/v1/warehouses/{result.Value!.Id}", result.Value)
                : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("CreateWarehouse");

        group.MapPut("/{id:guid}", async (Guid id, UpdateWarehouseRequest req, ISender sender) =>
        {
            var result = await sender.Send(new UpdateWarehouseCommand(id, req.Name, req.Address));

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ErrorCode == "NOT_FOUND"
                    ? Results.NotFound(new { result.ErrorCode, result.Message })
                    : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("UpdateWarehouse");

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteWarehouseCommand(id));
            return result.IsSuccess
                ? Results.NoContent()
                : Results.NotFound(new { result.ErrorCode, result.Message });
        }).WithName("DeleteWarehouse");

        return app;
    }

    public record CreateWarehouseRequest(
        string Code,
        string Name,
        WarehouseType Type,
        string? Address = null,
        Guid? ParentWarehouseId = null,
        string? MachineCode = null);

    public record UpdateWarehouseRequest(string Name, string? Address = null);
}
