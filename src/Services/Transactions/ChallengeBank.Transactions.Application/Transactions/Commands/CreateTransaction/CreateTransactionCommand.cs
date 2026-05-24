using ChallengeBank.BuildingBlocks.Application.Abstractions;
using ChallengeBank.BuildingBlocks.Application.Common;
using ChallengeBank.Transactions.Domain.Enums;

namespace ChallengeBank.Transactions.Application.Transactions.Commands.CreateTransaction;

public sealed record CreateTransactionCommand(
    Guid ClientId,
    decimal Amount,
    TransactionType Type,
    string? Description) : ICommand<Result<Guid>>;
