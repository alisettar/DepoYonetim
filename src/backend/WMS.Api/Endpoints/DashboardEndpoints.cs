using MediatR;
using WMS.Application.Dashboard.Queries;

namespace WMS.Api.Endpoints;

public static class DashboardEndpoints
{
    public static WebApplication MapDashboardEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/dashboard").WithTags("Dashboard");

        group.MapGet("/critical-stock", async (ISender sender) =>
        {
            var result = await sender.Send(new GetCriticalStockQuery());
            return Results.Ok(result.Value);
        }).WithName("GetCriticalStock");

        group.MapGet("/warehouse-fill", async (ISender sender) =>
        {
            var result = await sender.Send(new GetWarehouseFillQuery());
            return Results.Ok(result.Value);
        }).WithName("GetWarehouseFill");

        group.MapGet("/recent-movements", async (int count, ISender sender) =>
        {
            var result = await sender.Send(new GetRecentMovementsQuery(count));
            return Results.Ok(result.Value);
        }).WithName("GetRecentMovements");

        group.MapGet("/lot-search", async (string q, Guid? productId, ISender sender) =>
        {
            var result = await sender.Send(new LotSearchQuery(q, productId));
            return Results.Ok(result.Value);
        }).WithName("LotSearch");

        return app;
    }
}
