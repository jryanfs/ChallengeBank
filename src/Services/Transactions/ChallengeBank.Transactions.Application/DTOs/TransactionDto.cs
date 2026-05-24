using ChallengeBank.Transactions.Domain.Enums;

namespace ChallengeBank.Transactions.Application.DTOs;

public sealed record TransactionDto(
    Guid Id,
    Guid ClientId,
    decimal Amount,
    TransactionType Type,
    TransactionStatus Status,
    string? Description,
    DateTime CreatedAtUtc);
