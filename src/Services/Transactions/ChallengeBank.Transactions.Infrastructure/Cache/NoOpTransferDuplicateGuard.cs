using ChallengeBank.Transactions.Application.Abstractions;

namespace ChallengeBank.Transactions.Infrastructure.Cache;

public sealed class NoOpTransferDuplicateGuard : ITransferDuplicateGuard
{
    public Task<bool> TryRegisterAsync(
        Guid senderUserId,
        Guid receiverUserId,
        decimal amount,
        CancellationToken cancellationToken) =>
        Task.FromResult(true);

    public Task ReleaseAsync(
        Guid senderUserId,
        Guid receiverUserId,
        decimal amount,
        CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
