using System.Text.Json;
using WMS.Application.Identity.Commands;
using WMS.Shared.Result;

namespace WMS.Api.Endpoints.Auth;

public static class LoginEndpoint
{
    public static IEndpointRouteBuilder MapLoginEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/login", HandleRequest);
        return app;
    }

    private static async Task HandleRequest(HttpContext context)
    {
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var req = await context.Request.ReadFromJsonAsync<LoginRequest>(options, context.RequestAborted);
        if (req is null)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid request body." }, context.RequestAborted);
            return;
        }

        var mediator = context.RequestServices.GetRequiredService<MediatR.IMediator>();
        var command = new LoginCommand(req.Email, req.Password);
        var result = await mediator.Send(command, context.RequestAborted);

        if (result.IsSuccess)
        {
            context.Response.StatusCode = 200;
            await context.Response.WriteAsJsonAsync(result.Value, options, context.RequestAborted);
        }
        else
        {
            if (result.ErrorCode == "AUTH_FAILED")
            {
                context.Response.StatusCode = 401;
            }
            else
            {
                context.Response.StatusCode = 400;
            }
            await context.Response.WriteAsJsonAsync(new { message = result.Message }, options, context.RequestAborted);
        }
    }
}

public record LoginRequest(string Email, string Password);
