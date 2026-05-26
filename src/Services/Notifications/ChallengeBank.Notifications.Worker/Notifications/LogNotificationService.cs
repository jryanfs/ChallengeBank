using ChallengeBank.Contracts.Events;
using ChallengeBank.Contracts.Notifications;

namespace ChallengeBank.Notifications.Worker.Notifications;

public sealed class LogNotificationService(ILogger<LogNotificationService> logger) : INotificationService
{
    public Task SendBankingDetailsUpdatedAsync(ClientBankingDetailsUpdatedEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Notificação (stub): bankingDetails atualizado. ClientId={ClientId} Agency={Agency} Account={Account} UpdatedAtUtc={UpdatedAtUtc}",
            @event.ClientId,
            @event.Agency,
            @event.AccountNumber,
            @event.UpdatedAtUtc);

        return Task.CompletedTask;
    }
}

