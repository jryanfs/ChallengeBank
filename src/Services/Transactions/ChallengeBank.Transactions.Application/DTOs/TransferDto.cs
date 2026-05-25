using ChallengeBank.Transactions.Domain.Enums;

namespace ChallengeBank.Transactions.Application.DTOs;

public sealed record TransferDto(
    Guid Id,
    Guid SenderUserId,
    Guid ReceiverUserId,
    decimal Amount,
    string? Description,
    TransferStatus Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record CreateTransferResponseDto(Guid Id, TransferStatus Status);
