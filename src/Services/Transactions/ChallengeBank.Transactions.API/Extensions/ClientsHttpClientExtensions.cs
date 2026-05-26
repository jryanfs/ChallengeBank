using ChallengeBank.Transactions.API.Integration;
using ChallengeBank.Transactions.API.Options;
using ChallengeBank.Transactions.Application.Abstractions;
using Microsoft.Extensions.Http.Resilience;

namespace ChallengeBank.Transactions.API.Extensions;

public static class ClientsHttpClientExtensions
{
    /// <summary>
    /// Registra HttpClient para o microsserviço de Clientes com Polly:
    /// Retry, Circuit Breaker e Timeout (via Microsoft.Extensions.Http.Resilience).
    /// </summary>
    public static IServiceCollection AddClientsServiceHttpClient(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.Configure<ClientsServiceOptions>(configuration.GetSection(ClientsServiceOptions.SectionName));
        services.AddHttpContextAccessor();

        var options = configuration.GetSection(ClientsServiceOptions.SectionName).Get<ClientsServiceOptions>()
            ?? new ClientsServiceOptions();

        var httpClientBuilder = services
            .AddHttpClient(ClientsHttpClientNames.ClientsApi, client =>
            {
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
                client.Timeout = Timeout.InfiniteTimeSpan;
            });

        if (environment.IsDevelopment())
        {
            httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });
        }

        httpClientBuilder
            .AddStandardResilienceHandler(resilience =>
            {
                resilience.Retry.MaxRetryAttempts = options.RetryCount;
                resilience.Retry.Delay = TimeSpan.FromMilliseconds(options.RetryDelayMilliseconds);
                resilience.Retry.UseJitter = true;

                resilience.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                resilience.AttemptTimeout.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

                resilience.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(options.CircuitBreakerDurationSeconds);
                resilience.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(options.CircuitBreakerDurationSeconds);
                resilience.CircuitBreaker.FailureRatio = 0.5;
                resilience.CircuitBreaker.MinimumThroughput = options.CircuitBreakerFailures;
            });

        services.AddScoped<IClientExistenceChecker, HttpClientClientExistenceChecker>();

        return services;
    }
}
