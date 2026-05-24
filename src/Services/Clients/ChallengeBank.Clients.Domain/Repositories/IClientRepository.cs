using ChallengeBank.Clients.Domain.Entities;

namespace ChallengeBank.Clients.Domain.Repositories;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Client?> GetByDocumentAsync(string documentNumber, CancellationToken cancellationToken = default);

    Task AddAsync(Client client, CancellationToken cancellationToken = default);
}
