using ChallengeBank.Transactions.Application.Transfers.Commands.CreateTransfer;
using ChallengeBank.Transactions.Application.Transfers.Queries.GetTransferById;
using ChallengeBank.Transactions.Application.Transfers.Queries.GetTransfersByUserId;
using Microsoft.Extensions.DependencyInjection;

namespace ChallengeBank.Transactions.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateTransferCommandHandler>();
        services.AddScoped<GetTransferByIdQueryHandler>();
        services.AddScoped<GetTransfersByUserIdQueryHandler>();

        return services;
    }
}
