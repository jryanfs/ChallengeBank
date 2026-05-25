using ChallengeBank.BuildingBlocks.Application.Abstractions;
using ChallengeBank.BuildingBlocks.Application.Common;
using ChallengeBank.Transactions.Application.DTOs;
using ChallengeBank.Transactions.Application.Mapping;
using ChallengeBank.Transactions.Domain.Repositories;

namespace ChallengeBank.Transactions.Application.Transfers.Queries.GetTransferById;

public sealed class GetTransferByIdQueryHandler(ITransferRepository transferRepository)
    : IQueryHandler<GetTransferByIdQuery, Result<TransferDto>>
{
    public async Task<Result<TransferDto>> Handle(GetTransferByIdQuery query, CancellationToken cancellationToken)
    {
        var transfer = await transferRepository.GetByIdAsync(query.TransferId, cancellationToken);
        if (transfer is null)
            return Result.Failure<TransferDto>(
                Error.NotFound("Transfer.NotFound", $"Transferência com id '{query.TransferId}' não foi encontrada."));

        return Result.Success(TransferMapper.ToDto(transfer));
    }
}
