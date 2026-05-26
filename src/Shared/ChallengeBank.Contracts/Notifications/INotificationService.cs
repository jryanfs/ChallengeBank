using ChallengeBank.Contracts.Events;

namespace ChallengeBank.Contracts.Notifications;

public interface INotificationService
{
    Task SendBankingDetailsUpdatedAsync(ClientBankingDetailsUpdatedEvent @event, CancellationToken cancellationToken);
}

