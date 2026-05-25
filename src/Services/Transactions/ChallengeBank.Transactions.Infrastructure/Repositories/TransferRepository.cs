using ChallengeBank.Transactions.Domain.Entities;
using ChallengeBank.Transactions.Domain.Repositories;
using ChallengeBank.Transactions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChallengeBank.Transactions.Infrastructure.Repositories;

public sealed class TransferRepository(TransactionsDbContext context) : ITransferRepository
{
    public Task<Transfer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Transfers.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Transfer>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await context.Transfers
            .AsNoTracking()
            .Where(t => t.SenderUserId == userId || t.ReceiverUserId == userId)
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Transfer transfer, CancellationToken cancellationToken = default) =>
        await context.Transfers.AddAsync(transfer, cancellationToken);
}
