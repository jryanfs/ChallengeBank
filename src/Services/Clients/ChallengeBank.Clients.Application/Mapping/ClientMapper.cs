using ChallengeBank.Clients.Application.DTOs;
using ChallengeBank.Clients.Domain.Entities;

namespace ChallengeBank.Clients.Application.Mapping;

public static class ClientMapper
{
    public static ClientDto ToDto(Client client) =>
        new(
            client.Id,
            client.FullName,
            client.DocumentNumber,
            client.Email,
            client.Address is null
                ? null
                : new AddressDto(client.Address.Street, client.Address.City, client.Address.State, client.Address.PostalCode),
            client.BankingDetails is null
                ? null
                : new BankingDetailsDto(client.BankingDetails.Agency, client.BankingDetails.AccountNumber),
            client.Status,
            client.CreatedAtUtc,
            client.UpdatedAtUtc);
}
