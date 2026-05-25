using ChallengeBank.BuildingBlocks.Application.Abstractions;
using ChallengeBank.BuildingBlocks.Application.Common;
using ChallengeBank.BuildingBlocks.Application.Interfaces;
using ChallengeBank.BuildingBlocks.Domain.Exceptions;
using ChallengeBank.Clients.Domain.Entities;
using ChallengeBank.Clients.Domain.Repositories;
using ChallengeBank.Clients.Domain.ValueObjects;

namespace ChallengeBank.Clients.Application.Clients.Commands.CreateClient;

public sealed class CreateClientCommandHandler(
    IClientRepository clientRepository,
    IClientsUnitOfWork unitOfWork) : ICommandHandler<CreateClientCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateClientCommand command, CancellationToken cancellationToken)
    {
        var existing = await clientRepository.GetByDocumentAsync(command.DocumentNumber, cancellationToken);
        if (existing is not null)
            return Result.Failure<Guid>(Error.Conflict("Client.DocumentExists", "Já existe um cliente cadastrado com este documento."));

        try
        {
            Address? address = command.Address is null
                ? null
                : Address.Create(command.Address.Street, command.Address.City, command.Address.State, command.Address.PostalCode);

            BankingDetails? banking = command.BankingDetails is null
                ? null
                : BankingDetails.Create(command.BankingDetails.Agency, command.BankingDetails.AccountNumber);

            var client = Client.Create(command.FullName, command.DocumentNumber, command.Email, address, banking);
            await clientRepository.AddAsync(client, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(client.Id);
        }
        catch (DomainException ex)
        {
            return Result.Failure<Guid>(Error.Validation("Client.Invalid", ex.Message));
        }
    }
}
