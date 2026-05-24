using ChallengeBank.BuildingBlocks.Application.Abstractions;
using ChallengeBank.BuildingBlocks.Application.Common;
using ChallengeBank.Clients.Application.DTOs;

namespace ChallengeBank.Clients.Application.Clients.Queries.GetClientById;

public sealed record GetClientByIdQuery(Guid ClientId) : IQuery<Result<ClientDto>>;
