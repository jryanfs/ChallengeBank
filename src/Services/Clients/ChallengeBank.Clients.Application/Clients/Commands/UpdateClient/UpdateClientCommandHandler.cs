using ChallengeBank.BuildingBlocks.Application.Abstractions;
using ChallengeBank.BuildingBlocks.Application.Common;
using ChallengeBank.BuildingBlocks.Application.Interfaces;
using ChallengeBank.BuildingBlocks.Domain.Exceptions;
using ChallengeBank.Clients.Application.DTOs;
using ChallengeBank.Clients.Application.Mapping;
using ChallengeBank.Clients.Domain.Repositories;
using ChallengeBank.Clients.Domain.ValueObjects;

namespace ChallengeBank.Clients.Application.Clients.Commands.UpdateClient;

public sealed class UpdateClientCommandHandler(
    IClientRepository clientRepository,
    IClientsUnitOfWork unitOfWork) : ICommandHandler<UpdateClientCommand, Result<ClientDto>>
{
    public async Task<Result<ClientDto>> Handle(UpdateClientCommand command, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdForUpdateAsync(command.ClientId, cancellationToken);
        if (client is null)
            return Result.Failure<ClientDto>(
                Error.NotFound("Client.NotFound", $"Cliente com id '{command.ClientId}' não foi encontrado."));

        try
        {
            Address? address = command.Address is null
                ? null
                : Address.Create(command.Address.Street, command.Address.City, command.Address.State, command.Address.PostalCode);

            BankingDetails? banking = command.BankingDetails is null
                ? null
                : BankingDetails.Create(command.BankingDetails.Agency, command.BankingDetails.AccountNumber);

            client.UpdatePartial(
                command.Name,
                command.Email,
                address,
                banking,
                updateAddress: command.Address is not null,
                updateBankingDetails: command.BankingDetails is not null);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(ClientMapper.ToDto(client));
        }
        catch (DomainException ex)
        {
            return Result.Failure<ClientDto>(Error.Validation("Client.Invalid", ex.Message));
        }
    }
}
