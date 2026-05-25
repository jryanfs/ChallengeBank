using ChallengeBank.BuildingBlocks.Application.Abstractions;
using ChallengeBank.BuildingBlocks.Application.Common;
using ChallengeBank.Transactions.Application.DTOs;

namespace ChallengeBank.Transactions.Application.Transfers.Queries.GetTransferById;

public sealed record GetTransferByIdQuery(Guid TransferId) : IQuery<Result<TransferDto>>;
