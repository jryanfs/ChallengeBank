using ChallengeBank.Clients.Application.DTOs;

namespace ChallengeBank.Clients.Application.Abstractions;

public interface IClientCache
{
    Task<ClientDto?> GetByIdAsync(Guid clientId, CancellationToken cancellationToken);

    Task SetByIdAsync(Guid clientId, ClientDto dto, TimeSpan ttl, CancellationToken cancellationToken);

    Task RemoveByIdAsync(Guid clientId, CancellationToken cancellationToken);
}

