using System.Text.Json;
using WMS.Application.Admin.Commands;
using WMS.Application.Admin.Queries;
using WMS.Shared.Result;

namespace WMS.Api.Endpoints.Admin;

public static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/admin/tenants", HandleCreate);
        app.MapGet("/api/v1/admin/tenants", HandleList);
        app.MapPatch("/api/v1/admin/tenants/{id}/status", HandleStatus);
        app.MapGet("/api/v1/admin/tenants/{id}/health", HandleHealth);
        return app;
    }

    private static async Task HandleCreate(HttpContext context)
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var req = await context.Request.ReadFromJsonAsync<CreateTenantRequest>(options, context.RequestAborted);
        if (req is null)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid request body." }, context.RequestAborted);
            return;
        }

        var mediator = context.RequestServices.GetRequiredService<MediatR.IMediator>();
        var command = new CreateTenantCommand(req.Code, req.Name, req.Plan);
        var result = await mediator.Send(command, context.RequestAborted);

        if (result.IsSuccess)
        {
            context.Response.StatusCode = 201;
            await context.Response.WriteAsJsonAsync(result.Value, options, context.RequestAborted);
        }
        else
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { message = result.Message }, options, context.RequestAborted);
        }
    }

    private static async Task HandleList(HttpContext context)
    {
        var status = context.Request.Query["status"].ToString();
        var page = int.TryParse(context.Request.Query["page"], out var p) ? p : 1;
        var pageSize = int.TryParse(context.Request.Query["pageSize"], out var ps) ? ps : 50;

        var mediator = context.RequestServices.GetRequiredService<MediatR.IMediator>();
        var query = new ListTenantsQuery(status, page, pageSize);
        var result = await mediator.Send(query, context.RequestAborted);

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        if (result.IsSuccess)
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsJsonAsync(result.Value, options, context.RequestAborted);
        }
        else
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { message = result.Message }, options, context.RequestAborted);
        }
    }

    private static async Task HandleStatus(HttpContext context)
    {
        var idStr = context.Request.RouteValues["id"]?.ToString();
        if (!Guid.TryParse(idStr, out var id))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid tenant ID." }, context.RequestAborted);
            return;
        }

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var req = await context.Request.ReadFromJsonAsync<UpdateTenantStatusRequest>(options, context.RequestAborted);
        if (req is null)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid request body." }, context.RequestAborted);
            return;
        }

        var mediator = context.RequestServices.GetRequiredService<MediatR.IMediator>();
        var command = new UpdateTenantStatusCommand(id, req.Status);
        var result = await mediator.Send(command, context.RequestAborted);

        if (result.IsSuccess)
        {
            context.Response.StatusCode = 204;
        }
        else
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { message = result.Message }, options, context.RequestAborted);
        }
    }

    private static async Task HandleHealth(HttpContext context)
    {
        var idStr = context.Request.RouteValues["id"]?.ToString();
        if (!Guid.TryParse(idStr, out var id))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid tenant ID." }, context.RequestAborted);
            return;
        }

        var mediator = context.RequestServices.GetRequiredService<MediatR.IMediator>();
        var query = new TenantHealthQuery(id);
        var result = await mediator.Send(query, context.RequestAborted);

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        if (result.IsSuccess)
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsJsonAsync(result.Value, options, context.RequestAborted);
        }
        else
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { message = result.Message }, options, context.RequestAborted);
        }
    }
}

public record CreateTenantRequest(string Code, string Name, string? Plan = null);
public record UpdateTenantStatusRequest(string Status);
public record ListTenantsRequest(string? Status = null, int Page = 1, int PageSize = 50);

public record TenantResponse(
    Guid Id,
    string Code,
    string Name,
    string Status,
    string? Plan,
    DateTime CreatedAt);

public record TenantHealthResponse(
    Guid TenantId,
    string Status,
    string? ConnectionStatus,
    string? ErrorMessage);
