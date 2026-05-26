using ChallengeBank.Transactions.API.Integration;
using ChallengeBank.Transactions.Application.Abstractions;
using ChallengeBank.Transactions.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;

namespace ChallengeBank.Microservices.IntegrationTests.Infrastructure;

public sealed class TransactionsWebApplicationFactory : WebApplicationFactory<ChallengeBank.Transactions.API.Program>
{
    private const string InMemoryDatabaseName = "ChallengeBankTransactionsTests";
    private readonly InMemoryDatabaseRoot _databaseRoot = new();
    private readonly ClientsWebApplicationFactory _clientsFactory;

    public TransactionsWebApplicationFactory(ClientsWebApplicationFactory clientsFactory)
    {
        _clientsFactory = clientsFactory;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            RemoveDbContext<TransactionsDbContext>(services);

            services.AddDbContext<TransactionsDbContext>(options =>
                options.UseInMemoryDatabase(InMemoryDatabaseName, _databaseRoot));

            services.RemoveAll<IClientExistenceChecker>();

            services
                .AddHttpClient(ClientsHttpClientNames.ClientsApi, client =>
                {
                    client.BaseAddress = new Uri("http://localhost");
                    client.Timeout = Timeout.InfiniteTimeSpan;
                })
                .ConfigurePrimaryHttpMessageHandler(() => _clientsFactory.Server.CreateHandler())
                .AddStandardResilienceHandler();

            services.AddScoped<IClientExistenceChecker, HttpClientClientExistenceChecker>();
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionsDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    private static void RemoveDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(DbContextOptions<TContext>)
                        || d.ServiceType == typeof(TContext))
            .ToList();

        foreach (var descriptor in descriptors)
            services.Remove(descriptor);
    }
}
