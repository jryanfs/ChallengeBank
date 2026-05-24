using ChallengeBank.Transactions.Domain.Entities;
using ChallengeBank.Transactions.Domain.Repositories;
using ChallengeBank.Transactions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChallengeBank.Transactions.Infrastructure.Repositories;

public sealed class TransactionRepository(TransactionsDbContext context) : ITransactionRepository
{
    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Transactions.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Transaction>> GetByClientIdAsync(
        Guid clientId,
        CancellationToken cancellationToken = default) =>
        await context.Transactions
            .AsNoTracking()
            .Where(t => t.ClientId == clientId)
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default) =>
        await context.Transactions.AddAsync(transaction, cancellationToken);
}
