namespace ChallengeBank.BuildingBlocks.Domain.Abstractions;

public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj) =>
        obj is ValueObject other && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());

    public bool Equals(ValueObject? other) => Equals((object?)other);

    public override int GetHashCode() =>
        GetEqualityComponents()
            .Aggregate(0, (hash, component) => HashCode.Combine(hash, component?.GetHashCode() ?? 0));

    public static bool operator ==(ValueObject? left, ValueObject? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(ValueObject? left, ValueObject? right) => !(left == right);
}
