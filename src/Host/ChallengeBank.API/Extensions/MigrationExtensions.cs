using ChallengeBank.Clients.Infrastructure.Persistence;
using ChallengeBank.Transactions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChallengeBank.API.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()
            && !app.Environment.IsEnvironment("ChallengerBank"))
            return;

        using var scope = app.Services.CreateScope();

        var clientsDb = scope.ServiceProvider.GetRequiredService<ClientsDbContext>();
        await clientsDb.Database.MigrateAsync();

        var transactionsDb = scope.ServiceProvider.GetRequiredService<TransactionsDbContext>();
        await transactionsDb.Database.MigrateAsync();
    }
}
