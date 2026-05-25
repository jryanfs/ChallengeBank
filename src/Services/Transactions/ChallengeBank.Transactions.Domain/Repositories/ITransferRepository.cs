using ChallengeBank.Transactions.Domain.Entities;

namespace ChallengeBank.Transactions.Domain.Repositories;

public interface ITransferRepository
{
    Task<Transfer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Transfer>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(Transfer transfer, CancellationToken cancellationToken = default);
}
