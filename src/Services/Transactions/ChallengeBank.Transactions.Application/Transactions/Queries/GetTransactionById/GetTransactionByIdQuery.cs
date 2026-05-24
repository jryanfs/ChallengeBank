using ChallengeBank.BuildingBlocks.Application.Abstractions;
using ChallengeBank.BuildingBlocks.Application.Common;
using ChallengeBank.Transactions.Application.DTOs;

namespace ChallengeBank.Transactions.Application.Transactions.Queries.GetTransactionById;

public sealed record GetTransactionByIdQuery(Guid TransactionId) : IQuery<Result<TransactionDto>>;
