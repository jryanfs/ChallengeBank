using System.Net;
using System.Net.Http.Headers;
using ChallengeBank.Transactions.Application.Abstractions;
using ChallengeBank.Transactions.Application.Integration;

namespace ChallengeBank.Transactions.API.Integration;

/// <summary>
/// Valida existência de cliente via HTTP no microsserviço de Clientes (POST /api/transfers → GET /api/clients/{id}).
/// </summary>
public sealed class HttpClientClientExistenceChecker(
    IHttpClientFactory httpClientFactory,
    IHttpContextAccessor httpContextAccessor,
    ILogger<HttpClientClientExistenceChecker> logger) : IClientExistenceChecker
{
    public async Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient(ClientsHttpClientNames.ClientsApi);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/clients/{userId}");

        if (httpContextAccessor.HttpContext?.Request.Headers.Authorization is { } authHeader
            && !string.IsNullOrWhiteSpace(authHeader.ToString()))
        {
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(authHeader.ToString());
        }

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(request, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "Falha ao consultar microsserviço de clientes para o usuário {UserId}", userId);
            throw new ClientsServiceException("Não foi possível consultar o serviço de clientes.", ex);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
            return false;

        if (response.IsSuccessStatusCode)
            return true;

        logger.LogWarning(
            "Microsserviço de clientes retornou {StatusCode} para o usuário {UserId}",
            (int)response.StatusCode,
            userId);

        throw new ClientsServiceException(
            $"O serviço de clientes retornou status {(int)response.StatusCode}.");
    }
}
