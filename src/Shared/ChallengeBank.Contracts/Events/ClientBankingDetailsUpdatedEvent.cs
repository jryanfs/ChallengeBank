namespace ChallengeBank.Contracts.Events;

public sealed record ClientBankingDetailsUpdatedEvent(
    Guid ClientId,
    string Agency,
    string AccountNumber,
    DateTime UpdatedAtUtc,
    string? ClientEmail = null,
    string? ClientFullName = null);

