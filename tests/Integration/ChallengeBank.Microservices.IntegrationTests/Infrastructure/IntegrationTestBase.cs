using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ChallengeBank.Microservices.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IClassFixture<MicroservicesFixture>, IAsyncLifetime
{
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected IntegrationTestBase(MicroservicesFixture fixture, bool useTransactionsApi)
    {
        Fixture = fixture;
        Client = useTransactionsApi
            ? fixture.Transactions.CreateClient()
            : fixture.Clients.CreateClient();
        ClientsClient = fixture.Clients.CreateClient();
    }

    protected MicroservicesFixture Fixture { get; }

    protected HttpClient Client { get; }

    protected HttpClient ClientsClient { get; }

    public async Task InitializeAsync() => await Fixture.InitializeAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task<string> LoginAsAdminAsync(HttpClient? client = null) =>
        await LoginAsync("admin", "Admin@123", client ?? Client);

    protected async Task<string> LoginAsUserAsync(HttpClient? client = null) =>
        await LoginAsync("user", "User@123", client ?? Client);

    private static async Task<string> LoginAsync(string username, string password, HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { username, password });
        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelopeResponse<LoginData>>(JsonOptions);
        envelope.ShouldHaveSuccessStatus();
        return envelope!.Data!.AccessToken;
    }

    protected void SetBearerToken(string token, HttpClient? client = null)
    {
        var target = client ?? Client;
        target.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    protected static async Task<ApiEnvelopeResponse<T>> ReadEnvelopeAsync<T>(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<ApiEnvelopeResponse<T>>(JsonOptions))!;
}

internal static class ApiEnvelopeAssertions
{
    public static void ShouldHaveSuccessStatus<T>(this ApiEnvelopeResponse<T>? envelope)
    {
        if (envelope is null)
            throw new InvalidOperationException("Response envelope is null.");

        if (envelope.Status is < 200 or >= 300)
            throw new InvalidOperationException($"Expected success status, got {envelope.Status}: {envelope.Message}");
    }
}
