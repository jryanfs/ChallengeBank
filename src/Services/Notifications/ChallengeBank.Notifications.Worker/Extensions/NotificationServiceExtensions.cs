using ChallengeBank.Contracts.Notifications;
using ChallengeBank.Notifications.Worker.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChallengeBank.Notifications.Worker.Extensions;

public static class NotificationServiceExtensions
{
    public static IServiceCollection AddNotificationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SendGridOptions>(configuration.GetSection(SendGridOptions.SectionName));
        services.AddSingleton<SendGridNotificationService>();
        services.AddSingleton<INotificationService>(sp => sp.GetRequiredService<SendGridNotificationService>());
        return services;
    }

    public static void LogNotificationConfiguration(this IHost host)
    {
        var options = host.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<SendGridOptions>>().Value;
        var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("SendGrid");

        if (options.Enabled)
        {
            logger.LogInformation(
                "Notificações por e-mail: SendGrid ATIVO. From={From} OverrideTo={Override}",
                options.FromEmail,
                string.IsNullOrWhiteSpace(options.OverrideRecipientEmail) ? "(e-mail do cliente)" : options.OverrideRecipientEmail);
            return;
        }

        logger.LogWarning(
            "Notificações por e-mail: SendGrid INATIVO — apenas log. Configure SendGrid:ApiKey e SendGrid:FromEmail.");
    }
}
