using ChallengeBank.BuildingBlocks.Application.Abstractions;
using ChallengeBank.BuildingBlocks.Application.Common;
using ChallengeBank.BuildingBlocks.Application.Interfaces;
using ChallengeBank.BuildingBlocks.Domain.Exceptions;
using ChallengeBank.Transactions.Domain.Entities;
using ChallengeBank.Transactions.Domain.Repositories;

namespace ChallengeBank.Transactions.Application.Transactions.Commands.CreateTransaction;

public sealed class CreateTransactionCommandHandler(
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateTransactionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateTransactionCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var transaction = Transaction.Create(
                command.ClientId,
                command.Amount,
                command.Type,
                command.Description);

            transaction.Complete();

            await transactionRepository.AddAsync(transaction, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(transaction.Id);
        }
        catch (DomainException ex)
        {
            return Result.Failure<Guid>(Error.Validation("Transaction.Invalid", ex.Message));
        }
    }
}
