namespace ChallengeBank.Microservices.IntegrationTests.Infrastructure;

public sealed class ApiEnvelopeResponse<T>
{
    public int Status { get; init; }

    public string Message { get; init; } = string.Empty;

    public string Trace { get; init; } = string.Empty;

    public T? Data { get; init; }
}

public sealed class LoginData
{
    public string AccessToken { get; init; } = string.Empty;
}

public sealed class IdData
{
    public Guid Id { get; init; }
}

public sealed class CreateTransferData
{
    public Guid Id { get; init; }

    public int Status { get; init; }
}
