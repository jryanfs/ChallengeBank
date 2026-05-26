using ChallengeBank.BuildingBlocks.Application.Abstractions;
using ChallengeBank.BuildingBlocks.Application.Common;
using ChallengeBank.Clients.Application.Abstractions;
using ChallengeBank.Clients.Application.DTOs;
using ChallengeBank.Clients.Application.Mapping;
using ChallengeBank.Clients.Domain.Repositories;

namespace ChallengeBank.Clients.Application.Clients.Queries.GetClientById;

public sealed class GetClientByIdQueryHandler(IClientRepository clientRepository, IClientCache? cache = null)
    : IQueryHandler<GetClientByIdQuery, Result<ClientDto>>
{
    public async Task<Result<ClientDto>> Handle(GetClientByIdQuery query, CancellationToken cancellationToken)
    {
        if (cache is not null)
        {
            var cached = await cache.GetByIdAsync(query.ClientId, cancellationToken);
            if (cached is not null)
                return Result.Success(cached);
        }

        var client = await clientRepository.GetByIdAsync(query.ClientId, cancellationToken);
        if (client is null)
            return Result.Failure<ClientDto>(
                Error.NotFound("Client.NotFound", $"Cliente com id '{query.ClientId}' não foi encontrado."));

        var dto = ClientMapper.ToDto(client);

        if (cache is not null)
        {
            // TTL recomendado no checklist: 60–300s (cache de leitura estável)
            await cache.SetByIdAsync(query.ClientId, dto, TimeSpan.FromSeconds(120), cancellationToken);
        }

        return Result.Success(dto);
    }
}
