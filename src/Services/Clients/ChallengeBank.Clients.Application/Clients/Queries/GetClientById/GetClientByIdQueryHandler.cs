using ChallengeBank.BuildingBlocks.Application.Abstractions;
using ChallengeBank.BuildingBlocks.Application.Common;
using ChallengeBank.Clients.Application.DTOs;
using ChallengeBank.Clients.Application.Mapping;
using ChallengeBank.Clients.Domain.Repositories;

namespace ChallengeBank.Clients.Application.Clients.Queries.GetClientById;

public sealed class GetClientByIdQueryHandler(IClientRepository clientRepository)
    : IQueryHandler<GetClientByIdQuery, Result<ClientDto>>
{
    public async Task<Result<ClientDto>> Handle(GetClientByIdQuery query, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(query.ClientId, cancellationToken);
        if (client is null)
            return Result.Failure<ClientDto>(
                Error.NotFound("Client.NotFound", $"Cliente com id '{query.ClientId}' não foi encontrado."));

        return Result.Success(ClientMapper.ToDto(client));
    }
}
