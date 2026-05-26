using ChallengeBank.Clients.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace ChallengeBank.Microservices.IntegrationTests.Infrastructure;

public sealed class ClientsWebApplicationFactory : WebApplicationFactory<ChallengeBank.Clients.API.Program>
{
    private const string InMemoryDatabaseName = "ChallengeBankClientsTests";
    private readonly InMemoryDatabaseRoot _databaseRoot = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            RemoveDbContext<ClientsDbContext>(services);

            services.AddDbContext<ClientsDbContext>(options =>
                options.UseInMemoryDatabase(InMemoryDatabaseName, _databaseRoot));
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClientsDbContext>();
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
