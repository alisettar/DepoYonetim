using MediatR;
using WMS.Application.Catalog.Products.Commands;
using WMS.Application.Catalog.Products.Queries;
using WMS.Domain.Catalog;

namespace WMS.Api.Endpoints;

public static class ProductsEndpoints
{
    public static WebApplication MapProductsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/products").WithTags("Products");

        group.MapGet("/", async (ProductStatus? status, ISender sender) =>
        {
            var result = await sender.Send(new ListProductsQuery(status));
            return Results.Ok(result.Value);
        }).WithName("ListProducts");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetProductQuery(id));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { result.ErrorCode, result.Message });
        }).WithName("GetProduct");

        group.MapPost("/", async (CreateProductRequest req, ISender sender) =>
        {
            var result = await sender.Send(new CreateProductCommand(
                req.Code, req.Name, req.CategoryId, req.PrimaryUnitId,
                req.LotRequired, req.ShelfLifeDays, req.MinStock, req.MaxStock));
            return result.IsSuccess
                ? Results.Created($"/api/v1/products/{result.Value!.Id}", result.Value)
                : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("CreateProduct");

        group.MapPut("/{id:guid}", async (Guid id, UpdateProductRequest req, ISender sender) =>
        {
            var result = await sender.Send(new UpdateProductCommand(
                id, req.Name, req.CategoryId, req.LotRequired,
                req.ShelfLifeDays, req.MinStock, req.MaxStock));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ErrorCode == "NOT_FOUND"
                    ? Results.NotFound(new { result.ErrorCode, result.Message })
                    : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("UpdateProduct");

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteProductCommand(id));
            return result.IsSuccess
                ? Results.NoContent()
                : Results.NotFound(new { result.ErrorCode, result.Message });
        }).WithName("DeleteProduct");

        group.MapPost("/{id:guid}/units", async (Guid id, AddProductUnitRequest req, ISender sender) =>
        {
            var result = await sender.Send(new AddProductUnitCommand(id, req.UnitId, req.ConversionToPrimary));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ErrorCode == "NOT_FOUND"
                    ? Results.NotFound(new { result.ErrorCode, result.Message })
                    : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("AddProductUnit");

        group.MapDelete("/{id:guid}/units/{unitId:guid}", async (Guid id, Guid unitId, ISender sender) =>
        {
            var result = await sender.Send(new RemoveProductUnitCommand(id, unitId));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ErrorCode == "NOT_FOUND"
                    ? Results.NotFound(new { result.ErrorCode, result.Message })
                    : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("RemoveProductUnit");

        return app;
    }

    public record CreateProductRequest(
        string Code,
        string Name,
        Guid CategoryId,
        Guid PrimaryUnitId,
        bool LotRequired = false,
        int? ShelfLifeDays = null,
        decimal? MinStock = null,
        decimal? MaxStock = null);

    public record UpdateProductRequest(
        string Name,
        Guid CategoryId,
        bool LotRequired,
        int? ShelfLifeDays = null,
        decimal? MinStock = null,
        decimal? MaxStock = null);

    public record AddProductUnitRequest(Guid UnitId, decimal ConversionToPrimary);
}
