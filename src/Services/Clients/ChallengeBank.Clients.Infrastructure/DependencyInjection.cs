using ChallengeBank.BuildingBlocks.Application.Interfaces;
using ChallengeBank.Clients.Domain.Repositories;
using ChallengeBank.Clients.Infrastructure.Persistence;
using ChallengeBank.Clients.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChallengeBank.Clients.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ClientsDb")
            ?? throw new InvalidOperationException("Connection string 'ClientsDb' was not found.");

        services.AddDbContext<ClientsDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "clients")));

        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ClientsDbContext>());

        return services;
    }
}
