using ChallengeBank.BuildingBlocks.Application.Abstractions;
using ChallengeBank.BuildingBlocks.Application.Common;

namespace ChallengeBank.Clients.Application.Clients.Commands.CreateClient;

public sealed record CreateClientCommand(
    string FullName,
    string DocumentNumber,
    string Email) : ICommand<Result<Guid>>;
