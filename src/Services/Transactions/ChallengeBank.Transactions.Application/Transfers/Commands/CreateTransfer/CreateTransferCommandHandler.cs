using ChallengeBank.BuildingBlocks.Application.Abstractions;
using ChallengeBank.BuildingBlocks.Application.Common;
using ChallengeBank.BuildingBlocks.Application.Interfaces;
using ChallengeBank.BuildingBlocks.Domain.Exceptions;
using ChallengeBank.Transactions.Application.Abstractions;
using ChallengeBank.Transactions.Application.DTOs;
using ChallengeBank.Transactions.Application.Integration;
using ChallengeBank.Transactions.Domain.Entities;
using ChallengeBank.Transactions.Domain.Repositories;

namespace ChallengeBank.Transactions.Application.Transfers.Commands.CreateTransfer;

public sealed class CreateTransferCommandHandler(
    ITransferRepository transferRepository,
    IClientExistenceChecker clientExistenceChecker,
    ITransactionsUnitOfWork unitOfWork) : ICommandHandler<CreateTransferCommand, Result<CreateTransferResponseDto>>
{
    public async Task<Result<CreateTransferResponseDto>> Handle(
        CreateTransferCommand command,
        CancellationToken cancellationToken)
    {
        if (command.SenderUserId == Guid.Empty)
            return Result.Failure<CreateTransferResponseDto>(
                Error.Validation("Transfer.SenderRequired", "O identificador do remetente é obrigatório."));

        if (command.ReceiverUserId == Guid.Empty)
            return Result.Failure<CreateTransferResponseDto>(
                Error.Validation("Transfer.ReceiverRequired", "O identificador do destinatário é obrigatório."));

        try
        {
            if (!await clientExistenceChecker.ExistsAsync(command.SenderUserId, cancellationToken))
                return Result.Failure<CreateTransferResponseDto>(
                    Error.NotFound(
                        "Transfer.SenderNotFound",
                        $"Cliente remetente com id '{command.SenderUserId}' não foi encontrado."));

            if (!await clientExistenceChecker.ExistsAsync(command.ReceiverUserId, cancellationToken))
                return Result.Failure<CreateTransferResponseDto>(
                    Error.NotFound(
                        "Transfer.ReceiverNotFound",
                        $"Cliente destinatário com id '{command.ReceiverUserId}' não foi encontrado."));
        }
        catch (ClientsServiceException)
        {
            return Result.Failure<CreateTransferResponseDto>(
                Error.Failure("Transfer.ClientsServiceUnavailable", "O serviço de clientes está temporariamente indisponível."));
        }

        try
        {
            var transfer = Transfer.Create(
                command.SenderUserId,
                command.ReceiverUserId,
                command.Amount,
                command.Description);

            transfer.Complete();

            await transferRepository.AddAsync(transfer, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new CreateTransferResponseDto(transfer.Id, transfer.Status));
        }
        catch (DomainException ex)
        {
            return Result.Failure<CreateTransferResponseDto>(Error.Validation("Transfer.Invalid", ex.Message));
        }
    }
}
