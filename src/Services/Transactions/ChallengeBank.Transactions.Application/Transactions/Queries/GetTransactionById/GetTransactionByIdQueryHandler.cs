using ChallengeBank.BuildingBlocks.Application.Abstractions;
using ChallengeBank.BuildingBlocks.Application.Common;
using ChallengeBank.Transactions.Application.DTOs;
using ChallengeBank.Transactions.Domain.Repositories;

namespace ChallengeBank.Transactions.Application.Transactions.Queries.GetTransactionById;

public sealed class GetTransactionByIdQueryHandler(ITransactionRepository transactionRepository)
    : IQueryHandler<GetTransactionByIdQuery, Result<TransactionDto>>
{
    public async Task<Result<TransactionDto>> Handle(GetTransactionByIdQuery query, CancellationToken cancellationToken)
    {
        var transaction = await transactionRepository.GetByIdAsync(query.TransactionId, cancellationToken);
        if (transaction is null)
            return Result.Failure<TransactionDto>(Error.NotFound("Transaction.NotFound", "Transaction not found."));

        var dto = new TransactionDto(
            transaction.Id,
            transaction.ClientId,
            transaction.Amount,
            transaction.Type,
            transaction.Status,
            transaction.Description,
            transaction.CreatedAtUtc);

        return Result.Success(dto);
    }
}
