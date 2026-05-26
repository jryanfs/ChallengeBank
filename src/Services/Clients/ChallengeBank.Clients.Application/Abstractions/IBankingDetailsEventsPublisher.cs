using ChallengeBank.Contracts.Events;

namespace ChallengeBank.Clients.Application.Abstractions;

public interface IBankingDetailsEventsPublisher
{
    Task PublishUpdatedAsync(ClientBankingDetailsUpdatedEvent @event, CancellationToken cancellationToken);
}

