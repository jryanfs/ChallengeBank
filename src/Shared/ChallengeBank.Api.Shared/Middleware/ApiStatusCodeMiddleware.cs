using System.Text.Json;
using ChallengeBank.Api.Shared.Api;

namespace ChallengeBank.Api.Shared.Middleware;

public sealed class ApiStatusCodeMiddleware(RequestDelegate next)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = null };

    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        if (context.Response.HasStarted || !IsApiRequest(context))
            return;

        if (context.Response.StatusCode is not (StatusCodes.Status404NotFound or StatusCodes.Status405MethodNotAllowed))
            return;

        if (context.Response.ContentLength is > 0)
            return;

        if (context.Response.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
            return;

        var message = context.Response.StatusCode == StatusCodes.Status404NotFound
            ? ApiMessages.EndpointNotFound
            : ApiMessages.MethodNotAllowed;

        context.Response.ContentType = "application/json";
        var envelope = ApiEnvelope.Create(context, context.Response.StatusCode, message);
        await context.Response.WriteAsync(JsonSerializer.Serialize(envelope, JsonOptions));
    }

    private static bool IsApiRequest(HttpContext context) =>
        context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
}
