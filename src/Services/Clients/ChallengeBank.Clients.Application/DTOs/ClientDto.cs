using ChallengeBank.Clients.Domain.Enums;

namespace ChallengeBank.Clients.Application.DTOs;

public sealed record ClientDto(
    Guid Id,
    string FullName,
    string DocumentNumber,
    string Email,
    ClientStatus Status,
    DateTime CreatedAtUtc);
