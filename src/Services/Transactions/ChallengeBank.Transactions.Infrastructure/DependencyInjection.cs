using ChallengeBank.BuildingBlocks.Application.Interfaces;
using ChallengeBank.Transactions.Domain.Repositories;
using ChallengeBank.Transactions.Infrastructure.Persistence;
using ChallengeBank.Transactions.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ITransactionsUnitOfWork>(sp => sp.GetRequiredService<TransactionsDbContext>());

        return services;
    }
}
