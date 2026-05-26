namespace ChallengeBank.Microservices.IntegrationTests.Infrastructure;

public sealed class MicroservicesFixture : IAsyncLifetime, IDisposable
{
    public ClientsWebApplicationFactory Clients { get; } = new();

    public TransactionsWebApplicationFactory Transactions { get; }

    public MicroservicesFixture() => Transactions = new TransactionsWebApplicationFactory(Clients);

    public async Task InitializeAsync()
    {
        await Clients.ResetDatabaseAsync();
        await Transactions.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public void Dispose()
    {
        Transactions.Dispose();
        Clients.Dispose();
    }
}
