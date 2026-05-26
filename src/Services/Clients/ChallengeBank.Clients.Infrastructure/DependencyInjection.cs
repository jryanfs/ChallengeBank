using ChallengeBank.BuildingBlocks.Application.Interfaces;
using ChallengeBank.Clients.Application.Abstractions;
using ChallengeBank.Clients.Domain.Repositories;
using ChallengeBank.Clients.Infrastructure.Cache;
using ChallengeBank.Clients.Infrastructure.Messaging;
using ChallengeBank.Clients.Infrastructure.Persistence;
using ChallengeBank.Clients.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ChallengeBank.Clients.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ChallengeBankDb")
            ?? throw new InvalidOperationException("Connection string 'ChallengeBankDb' was not found.");

        services.AddDbContext<ClientsDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "clients")));

        var redisConnectionString = configuration["Redis:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var options = ConfigurationOptions.Parse(redisConnectionString);
                // Não derrubar a API se Redis estiver offline (ex.: dev local sem container).
                options.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(options);
            });
            services.AddScoped<IClientCache, RedisClientCache>();
        }

        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.AddScoped<IBankingDetailsEventsPublisher, RabbitMqBankingDetailsEventsPublisher>();

        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IClientsUnitOfWork>(sp => sp.GetRequiredService<ClientsDbContext>());

        return services;
    }
}
