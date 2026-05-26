namespace ChallengeBank.Notifications.Worker.Notifications;

public sealed class SendGridOptions
{
    public const string SectionName = "SendGrid";

    /// <summary>API Key do SendGrid (variável de ambiente ou User Secrets).</summary>
    public string ApiKey { get; init; } = string.Empty;

    public string FromEmail { get; init; } = string.Empty;

    public string FromName { get; init; } = "ChallengeBank";

    /// <summary>Se preenchido, todos os e-mails vão para este endereço (útil em dev/sandbox).</summary>
    public string? OverrideRecipientEmail { get; init; }

    public bool Enabled => !string.IsNullOrWhiteSpace(ApiKey)
                           && !string.IsNullOrWhiteSpace(FromEmail);
}
