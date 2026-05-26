using ChallengeBank.Transactions.Application.Abstractions;
using ChallengeBank.Transactions.Infrastructure.Cache;

namespace ChallengeBank.Transactions.API.Hosting;

public static class TransferDuplicateGuardDiagnostics
{
    public static void LogRegisteredGuard(WebApplication app)
    {
        if (app.Environment.IsEnvironment("Testing"))
            return;

        using var scope = app.Services.CreateScope();
        var guard = scope.ServiceProvider.GetRequiredService<ITransferDuplicateGuard>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("TransferDuplicateGuard");

        var guardName = guard.GetType().Name;
        if (guard is NoOpTransferDuplicateGuard)
        {
            logger.LogWarning(
                "Anti-duplicata DESATIVADO ({Guard}). Configure Redis:ConnectionString (ex.: redis:6379 no Docker).",
                guardName);
            return;
        }

        logger.LogInformation(
            "Anti-duplicata ATIVO ({Guard}). Janela={Minutes} min.",
            guardName,
            app.Configuration.GetValue("Redis:TransferDuplicateWindowMinutes", 5));
    }
}
