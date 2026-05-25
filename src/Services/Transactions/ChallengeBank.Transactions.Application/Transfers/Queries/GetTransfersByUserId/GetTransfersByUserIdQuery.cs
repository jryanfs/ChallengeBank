using ChallengeBank.BuildingBlocks.Application.Abstractions;
using ChallengeBank.BuildingBlocks.Application.Common;
using ChallengeBank.Transactions.Application.DTOs;

namespace ChallengeBank.Transactions.Application.Transfers.Queries.GetTransfersByUserId;

public sealed record GetTransfersByUserIdQuery(Guid UserId) : IQuery<Result<IReadOnlyList<TransferDto>>>;
