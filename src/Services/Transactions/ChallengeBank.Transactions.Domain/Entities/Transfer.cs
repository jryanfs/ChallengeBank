using ChallengeBank.BuildingBlocks.Domain.Abstractions;
using ChallengeBank.BuildingBlocks.Domain.Exceptions;
using ChallengeBank.Transactions.Domain.Enums;

namespace ChallengeBank.Transactions.Domain.Entities;

public sealed class Transfer : AggregateRoot<Guid>, IAuditableEntity
{
    private Transfer() { }

    public Guid SenderUserId { get; private set; }

    public Guid ReceiverUserId { get; private set; }

    public decimal Amount { get; private set; }

    public string? Description { get; private set; }

    public TransferStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    public static Transfer Create(Guid senderUserId, Guid receiverUserId, decimal amount, string? description = null)
    {
        if (senderUserId == Guid.Empty)
            throw new DomainException("O identificador do remetente é obrigatório.");

        if (receiverUserId == Guid.Empty)
            throw new DomainException("O identificador do destinatário é obrigatório.");

        if (senderUserId == receiverUserId)
            throw new DomainException("O remetente e o destinatário devem ser usuários diferentes.");

        if (amount <= 0)
            throw new DomainException("O valor da transferência deve ser maior que zero.");

        return new Transfer
        {
            Id = Guid.NewGuid(),
            SenderUserId = senderUserId,
            ReceiverUserId = receiverUserId,
            Amount = amount,
            Description = description?.Trim(),
            Status = TransferStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Complete()
    {
        if (Status == TransferStatus.Completed)
            throw new DomainException("A transferência já está concluída.");

        if (Status == TransferStatus.Cancelled)
            throw new DomainException("Transferências canceladas não podem ser concluídas.");

        Status = TransferStatus.Completed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Fail()
    {
        if (Status == TransferStatus.Completed)
            throw new DomainException("Transferências concluídas não podem falhar.");

        Status = TransferStatus.Failed;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
