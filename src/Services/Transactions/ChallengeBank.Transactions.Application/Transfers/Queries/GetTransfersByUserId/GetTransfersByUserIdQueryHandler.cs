using ChallengeBank.BuildingBlocks.Application.Abstractions;
using ChallengeBank.BuildingBlocks.Application.Common;
using ChallengeBank.Transactions.Application.Abstractions;
using ChallengeBank.Transactions.Application.DTOs;
using ChallengeBank.Transactions.Application.Mapping;
using ChallengeBank.Transactions.Domain.Repositories;

namespace ChallengeBank.Transactions.Application.Transfers.Queries.GetTransfersByUserId;

public sealed class GetTransfersByUserIdQueryHandler(
    ITransferRepository transferRepository,
    IClientExistenceChecker clientExistenceChecker)
    : IQueryHandler<GetTransfersByUserIdQuery, Result<IReadOnlyList<TransferDto>>>
{
    public async Task<Result<IReadOnlyList<TransferDto>>> Handle(
        GetTransfersByUserIdQuery query,
        CancellationToken cancellationToken)
    {
        if (query.UserId == Guid.Empty)
            return Result.Failure<IReadOnlyList<TransferDto>>(
                Error.Validation("Transfer.UserRequired", "O identificador do usuário é obrigatório."));

        if (!await clientExistenceChecker.ExistsAsync(query.UserId, cancellationToken))
            return Result.Failure<IReadOnlyList<TransferDto>>(
                Error.NotFound(
                    "Transfer.UserNotFound",
                    $"Cliente com id '{query.UserId}' não foi encontrado."));

        var transfers = await transferRepository.GetByUserIdAsync(query.UserId, cancellationToken);
        var dtos = transfers.Select(TransferMapper.ToDto).ToList();
        return Result.Success<IReadOnlyList<TransferDto>>(dtos);
    }
}
