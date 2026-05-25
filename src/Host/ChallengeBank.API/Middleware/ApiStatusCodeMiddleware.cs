using System.Text.Json;
using ChallengeBank.API.Api;

namespace ChallengeBank.API.Middleware;

/// <summary>
/// Ensures API routes always return the standard envelope when the pipeline sets 404/405 without a body.
/// </summary>
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
