using MediatR;
using WMS.Application.Catalog.Units.Commands;
using WMS.Application.Catalog.Units.Queries;

namespace WMS.Api.Endpoints;

public static class UnitsEndpoints
{
    public static WebApplication MapUnitsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/units").WithTags("Units");

        group.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new ListUnitsQuery());
            return Results.Ok(result.Value);
        }).WithName("ListUnits");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetUnitQuery(id));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { result.ErrorCode, result.Message });
        }).WithName("GetUnit");

        group.MapPost("/", async (CreateUnitRequest req, ISender sender) =>
        {
            var result = await sender.Send(new CreateUnitCommand(req.Code, req.Name));
            return result.IsSuccess
                ? Results.Created($"/api/v1/units/{result.Value!.Id}", result.Value)
                : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("CreateUnit");

        group.MapPut("/{id:guid}", async (Guid id, UpdateUnitRequest req, ISender sender) =>
        {
            var result = await sender.Send(new UpdateUnitCommand(id, req.Name));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ErrorCode == "NOT_FOUND"
                    ? Results.NotFound(new { result.ErrorCode, result.Message })
                    : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("UpdateUnit");

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteUnitCommand(id));
            return result.IsSuccess
                ? Results.NoContent()
                : Results.NotFound(new { result.ErrorCode, result.Message });
        }).WithName("DeleteUnit");

        return app;
    }

    public record CreateUnitRequest(string Code, string Name);
    public record UpdateUnitRequest(string Name);
}
