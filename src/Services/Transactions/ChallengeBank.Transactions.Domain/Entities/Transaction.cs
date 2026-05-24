using ChallengeBank.BuildingBlocks.Domain.Abstractions;
using ChallengeBank.BuildingBlocks.Domain.Exceptions;
using ChallengeBank.Transactions.Domain.Enums;

namespace ChallengeBank.Transactions.Domain.Entities;

public sealed class Transaction : AggregateRoot<Guid>, IAuditableEntity
{
    private Transaction() { }

    public Guid ClientId { get; private set; }

    public decimal Amount { get; private set; }

    public TransactionType Type { get; private set; }

    public TransactionStatus Status { get; private set; }

    public string? Description { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    public static Transaction Create(Guid clientId, decimal amount, TransactionType type, string? description = null)
    {
        if (clientId == Guid.Empty)
            throw new DomainException("Client id is required.");

        if (amount <= 0)
            throw new DomainException("Amount must be greater than zero.");

        return new Transaction
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            Amount = amount,
            Type = type,
            Status = TransactionStatus.Pending,
            Description = description?.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Complete()
    {
        if (Status == TransactionStatus.Completed)
            throw new DomainException("Transaction is already completed.");

        if (Status == TransactionStatus.Cancelled)
            throw new DomainException("Cancelled transactions cannot be completed.");

        Status = TransactionStatus.Completed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == TransactionStatus.Cancelled)
            throw new DomainException("Transaction is already cancelled.");

        if (Status == TransactionStatus.Completed)
            throw new DomainException("Completed transactions cannot be cancelled.");

        Status = TransactionStatus.Cancelled;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
