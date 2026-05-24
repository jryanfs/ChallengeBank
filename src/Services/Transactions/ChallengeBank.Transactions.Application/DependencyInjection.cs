using ChallengeBank.Transactions.Application.Transactions.Commands.CreateTransaction;
using ChallengeBank.Transactions.Application.Transactions.Queries.GetTransactionById;
using Microsoft.Extensions.DependencyInjection;

namespace ChallengeBank.Transactions.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateTransactionCommandHandler>();
        services.AddScoped<GetTransactionByIdQueryHandler>();

        return services;
    }
}
