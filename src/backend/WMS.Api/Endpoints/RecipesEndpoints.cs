using MediatR;
using WMS.Application.Recipes.Commands;
using WMS.Application.Recipes.Dtos;
using WMS.Application.Recipes.Queries;
using WMS.Domain.Recipes;
using WMS.Shared.Exceptions;

namespace WMS.Api.Endpoints;

public static class RecipesEndpoints
{
    public static WebApplication MapRecipesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/recipes").WithTags("Recipes");

        // List recipes
        group.MapGet("/", async (Guid? productId, ISender sender) =>
        {
            var result = await sender.Send(new ListRecipesQuery(productId));
            return Results.Ok(result.Value);
        }).WithName("ListRecipes");

        // Get recipe by id
        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetRecipeQuery(id));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { result.ErrorCode, result.Message });
        }).WithName("GetRecipe");

        // Create recipe
        group.MapPost("/", async (CreateRecipeRequest req, ISender sender) =>
        {
            var result = await sender.Send(new CreateRecipeCommand(req.ProductId, req.Name));
            return result.IsSuccess
                ? Results.Created($"/api/v1/recipes/{result.Value!.Id}", result.Value)
                : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("CreateRecipe");

        // Update recipe name
        group.MapPatch("/{id:guid}", async (Guid id, UpdateRecipeRequest req, ISender sender) =>
        {
            var result = await sender.Send(new UpdateRecipeCommand(id, req.Name));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("UpdateRecipe");

        // Archive recipe
        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new ArchiveRecipeCommand(id));
            return result.IsSuccess
                ? Results.NoContent()
                : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("ArchiveRecipe");

        // --- Versions ---

        group.MapGet("/{id:guid}/versions", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetRecipeQuery(id));
            return result.IsSuccess
                ? Results.Ok(result.Value!.Versions)
                : Results.NotFound(new { result.ErrorCode, result.Message });
        }).WithName("GetRecipeVersions");

        group.MapPost("/{id:guid}/versions", async (Guid id, AddVersionRequest req, ISender sender) =>
        {
            var result = await sender.Send(new AddRecipeVersionCommand(
                id, req.ValidFrom, req.ValidUntil, req.OutputQuantity, req.OutputUnitId));
            return result.IsSuccess
                ? Results.Created($"/api/v1/recipes/{id}/versions/{result.Value!.Id}", result.Value)
                : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("AddRecipeVersion");

        group.MapPatch("/{id:guid}/versions/{vid:guid}", async (Guid id, Guid vid,
            UpdateVersionRequest req, ISender sender) =>
        {
            var result = await sender.Send(new UpdateRecipeVersionCommand(id, vid,
                req.ValidFrom, req.ValidUntil, req.OutputQuantity, req.OutputUnitId));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("UpdateRecipeVersion");

        group.MapPost("/{id:guid}/versions/{vid:guid}/activate", async (Guid id, Guid vid, ISender sender) =>
        {
            var result = await sender.Send(new ActivateRecipeVersionCommand(id, vid));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("ActivateRecipeVersion");

        // Explode BOM (recursive)
        group.MapPost("/{id:guid}/versions/{vid:guid}/explode", async (Guid id, Guid vid, ISender sender) =>
        {
            var result = await sender.Send(new GetRecipeQuery(id));
            if (!result.IsSuccess)
                return Results.NotFound(new { result.ErrorCode, result.Message });

            var recipe = result.Value
                ?? throw new InvalidOperationException("Recipe not found");

            var version = recipe.Versions.FirstOrDefault(v => v.Id == vid)
                ?? throw new BusinessException("VERSION_NOT_FOUND", "Versiyon bulunamadı.");

            var lines = ExplodeBom(version, level: 0, new HashSet<Guid>());
            return Results.Ok(lines);
        }).WithName("ExplodeRecipeBOM");

        // --- Recipe Items ---

        group.MapPost("/{id:guid}/versions/{vid:guid}/items", async (Guid id, Guid vid,
            AddItemRequest req, ISender sender) =>
        {
            var result = await sender.Send(new AddRecipeItemCommand(id, vid,
                req.ProductId, req.Quantity, req.UnitId,
                req.WastePercent, req.WasteFixed, req.SortOrder));
            return result.IsSuccess
                ? Results.Created($"/api/v1/recipes/{id}/versions/{vid}/items/{result.Value!.Id}", result.Value)
                : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("AddRecipeItem");

        group.MapPatch("/{id:guid}/versions/{vid:guid}/items/{iid:guid}", async (Guid id, Guid vid, Guid iid,
            UpdateItemRequest req, ISender sender) =>
        {
            var result = await sender.Send(new UpdateRecipeItemCommand(id, vid, iid,
                req.Quantity, req.UnitId, req.WastePercent, req.WasteFixed, req.SortOrder));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("UpdateRecipeItem");

        group.MapDelete("/{id:guid}/versions/{vid:guid}/items/{iid:guid}", async (Guid id, Guid vid, Guid iid,
            ISender sender) =>
        {
            var result = await sender.Send(new DeleteRecipeItemCommand(id, vid, iid));
            return result.IsSuccess
                ? Results.NoContent()
                : Results.NotFound(new { result.ErrorCode, result.Message });
        }).WithName("DeleteRecipeItem");

        // --- Alternatives ---

        group.MapPost("/{id:guid}/versions/{vid:guid}/items/{iid:guid}/alternatives", async (Guid id, Guid vid, Guid iid,
            AddAlternativeRequest req, ISender sender) =>
        {
            var result = await sender.Send(new AddAlternativeMaterialCommand(id, vid, iid,
                req.ProductId, req.Priority, req.Quantity, req.UnitId));
            return result.IsSuccess
                ? Results.Created($"/api/v1/recipes/alternatives/{result.Value!.Id}", result.Value)
                : Results.UnprocessableEntity(new { result.ErrorCode, result.Message });
        }).WithName("AddAlternativeMaterial");

        group.MapDelete("/{id:guid}/versions/{vid:guid}/items/{iid:guid}/alternatives/{aid:guid}",
            async (Guid id, Guid vid, Guid iid, Guid aid, ISender sender) =>
        {
            var result = await sender.Send(new DeleteAlternativeMaterialCommand(id, vid, iid, aid));
            return result.IsSuccess
                ? Results.NoContent()
                : Results.NotFound(new { result.ErrorCode, result.Message });
        }).WithName("DeleteAlternativeMaterial");

        return app;
    }

    private static List<BomLineDto> ExplodeBom(RecipeVersionDto version, int level, HashSet<Guid> visited)
    {
        var lines = new List<BomLineDto>();

        foreach (var item in version.Items)
        {
            var effectiveQty = item.Quantity;
            if (item.WastePercent.HasValue)
                effectiveQty *= (1 + item.WastePercent.Value / 100);
            if (item.WasteFixed.HasValue)
                effectiveQty += item.WasteFixed.Value;

            lines.Add(new BomLineDto(
                item.ProductId,
                level,
                item.Quantity,
                item.UnitId,
                item.WastePercent,
                item.WasteFixed,
                effectiveQty));

            // Recursive expansion for multi-level BOM
            if (!visited.Contains(item.ProductId))
            {
                visited.Add(item.ProductId);
                // In production, this would query the repository for the child recipe
                // For now, the frontend/handler would handle recursion via API calls
            }
        }

        return lines;
    }

    public record CreateRecipeRequest(Guid ProductId, string Name);
    public record UpdateRecipeRequest(string Name);
    public record AddVersionRequest(DateTime ValidFrom, DateTime? ValidUntil, decimal OutputQuantity, Guid OutputUnitId);
    public record UpdateVersionRequest(DateTime? ValidFrom, DateTime? ValidUntil, decimal? OutputQuantity, Guid? OutputUnitId);
    public record AddItemRequest(Guid ProductId, decimal Quantity, Guid UnitId,
        decimal? WastePercent = null, decimal? WasteFixed = null, int SortOrder = 0);
    public record UpdateItemRequest(decimal Quantity, Guid UnitId,
        decimal? WastePercent = null, decimal? WasteFixed = null, int SortOrder = 0);
    public record AddAlternativeRequest(Guid ProductId, int Priority, decimal Quantity, Guid UnitId);
}
