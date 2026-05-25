using ChallengeBank.BuildingBlocks.Application.Abstractions;
using ChallengeBank.BuildingBlocks.Application.Common;
using ChallengeBank.Transactions.Application.DTOs;

namespace ChallengeBank.Transactions.Application.Transfers.Commands.CreateTransfer;

public sealed record CreateTransferCommand(
    Guid SenderUserId,
    Guid ReceiverUserId,
    decimal Amount,
    string? Description) : ICommand<Result<CreateTransferResponseDto>>;
