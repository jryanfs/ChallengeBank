using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ChallengeBank.API.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IClassFixture<ChallengeBankWebApplicationFactory>, IAsyncLifetime
{
    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected IntegrationTestBase(ChallengeBankWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    protected ChallengeBankWebApplicationFactory Factory { get; }

    protected HttpClient Client { get; }

    public async Task InitializeAsync() => await Factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task<string> LoginAsAdminAsync() => await LoginAsync("admin", "Admin@123");

    protected async Task<string> LoginAsUserAsync() => await LoginAsync("user", "User@123");

    private async Task<string> LoginAsync(string username, string password)
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new { username, password });
        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelopeResponse<LoginData>>(JsonOptions);
        envelope.ShouldHaveSuccessStatus();
        return envelope!.Data!.AccessToken;
    }

    protected void SetBearerToken(string token) =>
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
