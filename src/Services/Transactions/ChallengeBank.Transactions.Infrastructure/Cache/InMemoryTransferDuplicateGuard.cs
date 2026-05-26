using System.Collections.Concurrent;
using ChallengeBank.Transactions.Application.Abstractions;
using Microsoft.Extensions.Configuration;

namespace ChallengeBank.Transactions.Infrastructure.Cache;

public sealed class InMemoryTransferDuplicateGuard(IConfiguration configuration) : ITransferDuplicateGuard
{
    private readonly ConcurrentDictionary<string, DateTime> _entries = new();

    public Task<bool> TryRegisterAsync(
        Guid senderUserId,
        Guid receiverUserId,
        decimal amount,
        CancellationToken cancellationToken)
    {
        PurgeExpired();
        var key = TransferDuplicateKey.Build(senderUserId, receiverUserId, amount, Prefix);
        var expiresAt = DateTime.UtcNow.Add(Window);

        var added = _entries.TryAdd(key, expiresAt);
        return Task.FromResult(added);
    }

    public Task ReleaseAsync(
        Guid senderUserId,
        Guid receiverUserId,
        decimal amount,
        CancellationToken cancellationToken)
    {
        var key = TransferDuplicateKey.Build(senderUserId, receiverUserId, amount, Prefix);
        _entries.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    private void PurgeExpired()
    {
        var now = DateTime.UtcNow;
        foreach (var pair in _entries)
        {
            if (pair.Value <= now)
                _entries.TryRemove(pair.Key, out _);
        }
    }

    private string? Prefix => configuration["Redis:KeyPrefix"];

    private TimeSpan Window => TimeSpan.FromMinutes(
        configuration.GetValue("Redis:TransferDuplicateWindowMinutes", 5));
}
