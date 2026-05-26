using ChallengeBank.Transactions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChallengeBank.Transactions.API.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyTransactionsMigrationsAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()
            && !app.Environment.IsEnvironment("ChallengerBank")
            && !app.Environment.IsEnvironment("Docker"))
            return;

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TransactionsDbContext>();
        await db.Database.MigrateAsync();
    }
}
