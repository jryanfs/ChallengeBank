namespace ChallengeBank.BuildingBlocks.Domain.Abstractions;

public interface IAuditableEntity
{
    DateTime CreatedAtUtc { get; }
    DateTime? UpdatedAtUtc { get; }
}
