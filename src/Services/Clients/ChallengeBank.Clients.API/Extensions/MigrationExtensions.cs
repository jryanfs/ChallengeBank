using ChallengeBank.Clients.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChallengeBank.Clients.API.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyClientsMigrationsAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()
            && !app.Environment.IsEnvironment("ChallengerBank")
            && !app.Environment.IsEnvironment("Docker"))
            return;

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClientsDbContext>();
        await db.Database.MigrateAsync();
    }
}
