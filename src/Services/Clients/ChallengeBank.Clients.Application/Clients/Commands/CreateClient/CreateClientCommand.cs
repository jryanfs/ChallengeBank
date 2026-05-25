using ChallengeBank.BuildingBlocks.Application.Abstractions;
using ChallengeBank.BuildingBlocks.Application.Common;
using ChallengeBank.Clients.Application.DTOs;

namespace ChallengeBank.Clients.Application.Clients.Commands.CreateClient;

public sealed record CreateClientCommand(
    string FullName,
    string DocumentNumber,
    string Email,
    AddressDto? Address = null,
    BankingDetailsDto? BankingDetails = null) : ICommand<Result<Guid>>;
