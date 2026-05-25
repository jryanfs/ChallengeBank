using ChallengeBank.BuildingBlocks.Application.Abstractions;
using ChallengeBank.BuildingBlocks.Application.Common;
using ChallengeBank.Clients.Application.DTOs;

namespace ChallengeBank.Clients.Application.Clients.Commands.UpdateClient;

public sealed record UpdateClientCommand(
    Guid ClientId,
    string? Name,
    string? Email,
    AddressDto? Address,
    BankingDetailsDto? BankingDetails) : ICommand<Result<ClientDto>>;
