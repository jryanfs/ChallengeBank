using ChallengeBank.BuildingBlocks.Application.Abstractions;
using ChallengeBank.BuildingBlocks.Application.Common;
using ChallengeBank.Clients.Application.DTOs;
using ChallengeBank.Clients.Domain.Repositories;

namespace ChallengeBank.Clients.Application.Clients.Queries.GetClientById;

public sealed class GetClientByIdQueryHandler(IClientRepository clientRepository)
    : IQueryHandler<GetClientByIdQuery, Result<ClientDto>>
{
    public async Task<Result<ClientDto>> Handle(GetClientByIdQuery query, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(query.ClientId, cancellationToken);
        if (client is null)
            return Result.Failure<ClientDto>(Error.NotFound("Client.NotFound", "Client not found."));

        var dto = new ClientDto(
            client.Id,
            client.FullName,
            client.DocumentNumber,
            client.Email,
            client.Status,
            client.CreatedAtUtc);

        return Result.Success(dto);
    }
}
