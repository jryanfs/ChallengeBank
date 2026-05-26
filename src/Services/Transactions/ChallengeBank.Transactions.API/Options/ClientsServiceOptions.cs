namespace ChallengeBank.Transactions.API.Options;

public sealed class ClientsServiceOptions
{
    public const string SectionName = "ClientsService";

    public string BaseUrl { get; init; } = "http://localhost:5101";

    /// <summary>Timeout por tentativa HTTP (Polly TotalRequestTimeout).</summary>
    public int TimeoutSeconds { get; init; } = 2;

    /// <summary>Número de novas tentativas após falha transitória.</summary>
    public int RetryCount { get; init; } = 3;

    public int RetryDelayMilliseconds { get; init; } = 200;

    /// <summary>Falhas consecutivas para abrir o circuit breaker.</summary>
    public int CircuitBreakerFailures { get; init; } = 5;

    public int CircuitBreakerDurationSeconds { get; init; } = 30;
}
