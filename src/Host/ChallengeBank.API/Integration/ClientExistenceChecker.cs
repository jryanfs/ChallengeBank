using ChallengeBank.Clients.Domain.Repositories;
using ChallengeBank.Transactions.Application.Abstractions;

namespace ChallengeBank.API.Integration;

public sealed class ClientExistenceChecker(IClientRepository clientRepository) : IClientExistenceChecker
{
    public Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default) =>
        clientRepository.ExistsAsync(userId, cancellationToken);
}
