using ChallengeBank.Clients.Infrastructure.Persistence;
using ChallengeBank.Transactions.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace ChallengeBank.API.IntegrationTests.Infrastructure;

public sealed class ChallengeBankWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string InMemoryDatabaseName = "ChallengeBankIntegrationTests";
    private readonly InMemoryDatabaseRoot _databaseRoot = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            RemoveDbContext<ClientsDbContext>(services);
            RemoveDbContext<TransactionsDbContext>(services);

            services.AddDbContext<ClientsDbContext>(options =>
                options.UseInMemoryDatabase(InMemoryDatabaseName, _databaseRoot));

            services.AddDbContext<TransactionsDbContext>(options =>
                options.UseInMemoryDatabase(InMemoryDatabaseName, _databaseRoot));
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var clientsDb = scope.ServiceProvider.GetRequiredService<ClientsDbContext>();
        var transactionsDb = scope.ServiceProvider.GetRequiredService<TransactionsDbContext>();

        await clientsDb.Database.EnsureDeletedAsync();
        await transactionsDb.Database.EnsureDeletedAsync();
        await clientsDb.Database.EnsureCreatedAsync();
        await transactionsDb.Database.EnsureCreatedAsync();
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
