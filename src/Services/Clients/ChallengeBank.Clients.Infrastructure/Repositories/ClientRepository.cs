using ChallengeBank.Clients.Domain.Entities;
using ChallengeBank.Clients.Domain.Repositories;
using ChallengeBank.Clients.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChallengeBank.Clients.Infrastructure.Repositories;

public sealed class ClientRepository(ClientsDbContext context) : IClientRepository
{
    public Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<Client?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Clients.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Clients.AsNoTracking().AnyAsync(c => c.Id == id, cancellationToken);

    public Task<Client?> GetByDocumentAsync(string documentNumber, CancellationToken cancellationToken = default) =>
        context.Clients.FirstOrDefaultAsync(c => c.DocumentNumber == documentNumber, cancellationToken);

    public async Task AddAsync(Client client, CancellationToken cancellationToken = default) =>
        await context.Clients.AddAsync(client, cancellationToken);
}
