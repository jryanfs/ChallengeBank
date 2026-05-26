using ChallengeBank.BuildingBlocks.Application.Interfaces;
using ChallengeBank.Transactions.Application.Abstractions;
using ChallengeBank.Transactions.Domain.Repositories;
using ChallengeBank.Transactions.Infrastructure.Cache;
using ChallengeBank.Transactions.Infrastructure.Persistence;
using ChallengeBank.Transactions.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace ChallengeBank.Transactions.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ChallengeBankDb")
            ?? throw new InvalidOperationException("Connection string 'ChallengeBankDb' was not found.");

        services.AddDbContext<TransactionsDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "transactions")));

        RegisterTransferDuplicateGuard(services, configuration);

        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddScoped<ITransactionsUnitOfWork>(sp => sp.GetRequiredService<TransactionsDbContext>());

        return services;
    }

    private static void RegisterTransferDuplicateGuard(IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("Redis:UseInMemoryDuplicateGuard"))
        {
            services.AddSingleton<ITransferDuplicateGuard, InMemoryTransferDuplicateGuard>();
            return;
        }

        var redisConnectionString = configuration["Redis:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddSingleton<IConnectionMultiplexer>(_ =>
            {
                var options = ConfigurationOptions.Parse(redisConnectionString);
                options.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(options);
            });
            services.AddSingleton<ITransferDuplicateGuard, RedisTransferDuplicateGuard>();
            return;
        }

        services.AddSingleton<ITransferDuplicateGuard, NoOpTransferDuplicateGuard>();
    }
}
