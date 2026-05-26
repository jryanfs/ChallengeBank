using ChallengeBank.Contracts.Events;
using ChallengeBank.Contracts.Notifications;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace ChallengeBank.Notifications.Worker.Notifications;

public sealed class SendGridNotificationService(
    IOptions<SendGridOptions> options,
    ILogger<SendGridNotificationService> logger) : INotificationService
{
    private readonly SendGridOptions _options = options.Value;

    public async Task SendBankingDetailsUpdatedAsync(
        ClientBankingDetailsUpdatedEvent @event,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            LogFallback(@event, "SendGrid desabilitado (configure SendGrid:ApiKey e SendGrid:FromEmail).");
            return;
        }

        var toEmail = ResolveRecipientEmail(@event);
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            LogFallback(@event, "Destinatário de e-mail não informado no evento.");
            return;
        }

        var client = new SendGridClient(_options.ApiKey);
        var from = new EmailAddress(_options.FromEmail, _options.FromName);
        var to = new EmailAddress(toEmail, @event.ClientFullName ?? "Cliente");
        var subject = "ChallengeBank — dados bancários atualizados";
        var plain = BuildPlainText(@event);
        var html = BuildHtml(@event);

        var message = MailHelper.CreateSingleEmail(from, to, subject, plain, html);
        var response = await client.SendEmailAsync(message, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation(
                "E-mail SendGrid enviado. To={To} ClientId={ClientId} Status={Status}",
                toEmail,
                @event.ClientId,
                response.StatusCode);
            return;
        }

        var body = await response.Body.ReadAsStringAsync(cancellationToken);
        logger.LogError(
            "Falha ao enviar e-mail SendGrid. Status={Status} Body={Body} ClientId={ClientId}",
            response.StatusCode,
            body,
            @event.ClientId);
    }

    private string? ResolveRecipientEmail(ClientBankingDetailsUpdatedEvent @event)
    {
        if (!string.IsNullOrWhiteSpace(_options.OverrideRecipientEmail))
            return _options.OverrideRecipientEmail.Trim();

        return string.IsNullOrWhiteSpace(@event.ClientEmail) ? null : @event.ClientEmail.Trim();
    }

    private void LogFallback(ClientBankingDetailsUpdatedEvent @event, string reason)
    {
        logger.LogWarning(
            "{Reason} Notificação (log): bankingDetails atualizado. ClientId={ClientId} Email={Email} Agency={Agency} Account={Account}",
            reason,
            @event.ClientId,
            @event.ClientEmail,
            @event.Agency,
            @event.AccountNumber);
    }

    private static string BuildPlainText(ClientBankingDetailsUpdatedEvent @event)
    {
        var name = string.IsNullOrWhiteSpace(@event.ClientFullName) ? "Cliente" : @event.ClientFullName;
        return $"""
            Olá, {name}!

            Seus dados bancários no ChallengeBank foram atualizados.

            Agência: {@event.Agency}
            Conta: {@event.AccountNumber}
            Data (UTC): {@event.UpdatedAtUtc:yyyy-MM-dd HH:mm:ss}

            Se você não reconhece esta alteração, entre em contato com o banco imediatamente.
            """;
    }

    private static string BuildHtml(ClientBankingDetailsUpdatedEvent @event)
    {
        var name = string.IsNullOrWhiteSpace(@event.ClientFullName) ? "Cliente" : @event.ClientFullName;
        return $"""
            <p>Olá, <strong>{name}</strong>!</p>
            <p>Seus dados bancários no <strong>ChallengeBank</strong> foram atualizados.</p>
            <ul>
              <li><strong>Agência:</strong> {@event.Agency}</li>
              <li><strong>Conta:</strong> {@event.AccountNumber}</li>
              <li><strong>Data (UTC):</strong> {@event.UpdatedAtUtc:yyyy-MM-dd HH:mm:ss}</li>
            </ul>
            <p>Se você não reconhece esta alteração, entre em contato com o banco imediatamente.</p>
            """;
    }
}
