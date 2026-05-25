using ChallengeBank.BuildingBlocks.Domain.Abstractions;

namespace ChallengeBank.Clients.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    private Address(string? street, string? city, string? state, string? postalCode)
    {
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
    }

    public string? Street { get; }

    public string? City { get; }

    public string? State { get; }

    public string? PostalCode { get; }

    public static Address Create(string? street, string? city, string? state, string? postalCode) =>
        new(
            string.IsNullOrWhiteSpace(street) ? null : street.Trim(),
            string.IsNullOrWhiteSpace(city) ? null : city.Trim(),
            string.IsNullOrWhiteSpace(state) ? null : state.Trim(),
            string.IsNullOrWhiteSpace(postalCode) ? null : postalCode.Trim());

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return PostalCode;
    }
}
