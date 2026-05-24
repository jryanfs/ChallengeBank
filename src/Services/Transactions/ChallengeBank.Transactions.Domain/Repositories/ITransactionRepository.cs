using ChallengeBank.Transactions.Domain.Entities;

namespace ChallengeBank.Transactions.Domain.Repositories;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Transaction>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);

    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
}
