namespace ChallengeBank.API.Api;

/// <summary>
/// Standard API response envelope.
/// </summary>
public sealed class ApiEnvelope
{
    public int Status { get; init; }

    public string Message { get; init; } = string.Empty;

    public string Trace { get; init; } = string.Empty;

    public object? Data { get; init; }

    public static ApiEnvelope Create(HttpContext httpContext, int status, string message, object? data = null) =>
        new()
        {
            Status = status,
            Message = message,
            Trace = httpContext.TraceIdentifier,
            Data = data
        };
}
