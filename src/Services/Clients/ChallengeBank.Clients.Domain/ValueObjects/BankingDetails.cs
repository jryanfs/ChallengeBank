using ChallengeBank.BuildingBlocks.Domain.Abstractions;
using ChallengeBank.BuildingBlocks.Domain.Exceptions;

namespace ChallengeBank.Clients.Domain.ValueObjects;

public sealed class BankingDetails : ValueObject
{
    private BankingDetails(string agency, string accountNumber)
    {
        Agency = agency;
        AccountNumber = accountNumber;
    }

    public string Agency { get; }

    public string AccountNumber { get; }

    public static BankingDetails Create(string agency, string accountNumber)
    {
        if (string.IsNullOrWhiteSpace(agency))
            throw new DomainException("A agência bancária é obrigatória.");

        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new DomainException("O número da conta bancária é obrigatório.");

        return new BankingDetails(agency.Trim(), accountNumber.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Agency;
        yield return AccountNumber;
    }
}
