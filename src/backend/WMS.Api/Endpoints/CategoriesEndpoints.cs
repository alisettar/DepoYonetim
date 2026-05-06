using MediatR;
using WMS.Application.Catalog.Categories.Commands;
using WMS.Application.Catalog.Categories.Queries;

namespace WMS.Api.Endpoints;

public static class CategoriesEndpoints
{
    public static WebApplication MapCategoriesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/categories").WithTags("Categories");

        group.MapGet("/", async (ISender sender) =>
        {
            var result = await sender.Send(new ListCategoriesQuery());
            return Results.Ok(result.Value);
        }).WithName("ListCategories");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetCategoryQuery(id));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { result.ErrorCode, result.Message });
        }).WithName("GetCategory");

        group.MapPost("/", async (CreateCategoryRequest req, ISender sender) =>
        {
            var result = await sender.Send(new CreateCategoryCommand(req.Code, req.Name));
            return result.IsSuccess
                ? Results.Created($"/api/v1/categories/{result.Value!.Id}", result.Value)
                : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("CreateCategory");

        group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryRequest req, ISender sender) =>
        {
            var result = await sender.Send(new UpdateCategoryCommand(id, req.Name));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ErrorCode == "NOT_FOUND"
                    ? Results.NotFound(new { result.ErrorCode, result.Message })
                    : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("UpdateCategory");

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new DeleteCategoryCommand(id));
            return result.IsSuccess
                ? Results.NoContent()
                : Results.NotFound(new { result.ErrorCode, result.Message });
        }).WithName("DeleteCategory");

        return app;
    }

    public record CreateCategoryRequest(string Code, string Name);
    public record UpdateCategoryRequest(string Name);
}
