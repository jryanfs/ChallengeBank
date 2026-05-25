using ChallengeBank.Clients.Domain.Enums;

namespace ChallengeBank.Clients.Application.DTOs;

public sealed record ClientDto(
    Guid Id,
    string FullName,
    string DocumentNumber,
    string Email,
    AddressDto? Address,
    BankingDetailsDto? BankingDetails,
    ClientStatus Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
