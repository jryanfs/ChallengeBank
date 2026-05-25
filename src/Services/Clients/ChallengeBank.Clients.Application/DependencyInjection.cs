using ChallengeBank.Clients.Application.Clients.Commands.CreateClient;
using ChallengeBank.Clients.Application.Clients.Commands.UpdateClient;
using ChallengeBank.Clients.Application.Clients.Queries.GetClientById;
using Microsoft.Extensions.DependencyInjection;

namespace ChallengeBank.Clients.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateClientCommandHandler>();
        services.AddScoped<UpdateClientCommandHandler>();
        services.AddScoped<GetClientByIdQueryHandler>();

        return services;
    }
}
