namespace ChallengeBank.BuildingBlocks.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
