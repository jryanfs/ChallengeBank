using System.Net;
using System.Net.Http.Json;
using ChallengeBank.Clients.Application.DTOs;
using ChallengeBank.Microservices.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace ChallengeBank.Microservices.IntegrationTests.Flows;

public sealed class ClientFlowIntegrationTests(MicroservicesFixture fixture)
    : IntegrationTestBase(fixture, useTransactionsApi: false)
{
    [Fact]
    public async Task CreateAndGetClient_WithAuthenticatedUser_ReturnsEnvelopeWithClient()
    {
        var token = await LoginAsUserAsync();
        SetBearerToken(token);

        var createResponse = await Client.PostAsJsonAsync("/api/clients", new
        {
            fullName = "Maria Silva",
            documentNumber = "12345678901",
            email = "maria@challengebank.com",
            bankingDetails = new { agency = "0001", accountNumber = "12345-6" }
        });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdEnvelope = await ReadEnvelopeAsync<IdData>(createResponse);
        createdEnvelope.Status.Should().Be(201);
        createdEnvelope.Message.Should().Contain("sucesso");
        createdEnvelope.Trace.Should().NotBeNullOrWhiteSpace();

        var getResponse = await Client.GetAsync($"/api/clients/{createdEnvelope.Data!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getEnvelope = await ReadEnvelopeAsync<ClientDto>(getResponse);
        getEnvelope.Data!.FullName.Should().Be("Maria Silva");
        getEnvelope.Data.BankingDetails!.Agency.Should().Be("0001");
    }

    [Fact]
    public async Task GetClient_WhenIdDoesNotExist_ReturnsNotFoundEnvelopeWithMessage()
    {
        var token = await LoginAsAdminAsync();
        SetBearerToken(token);

        var response = await Client.GetAsync($"/api/clients/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var envelope = await ReadEnvelopeAsync<object>(response);
        envelope.Status.Should().Be(404);
        envelope.Message.Should().NotBeNullOrWhiteSpace();
        envelope.Message.Should().Contain("não foi encontrado");
        envelope.Trace.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task PatchClient_AsAdmin_ReturnsOkEnvelope()
    {
        var token = await LoginAsAdminAsync();
        SetBearerToken(token);

        var createResponse = await Client.PostAsJsonAsync("/api/clients", new
        {
            fullName = "Admin Patch Test",
            documentNumber = "44455566677",
            email = "adminpatch@challengebank.com"
        });

        var created = await ReadEnvelopeAsync<IdData>(createResponse);

        var patchResponse = await Client.PatchAsJsonAsync($"/api/clients/{created.Data!.Id}", new { name = "Updated Admin" });
        patchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var patchEnvelope = await ReadEnvelopeAsync<ClientDto>(patchResponse);
        patchEnvelope.Message.Should().Contain("atualizado");
        patchEnvelope.Data!.FullName.Should().Be("Updated Admin");
    }

    [Fact]
    public async Task PatchClient_AsUser_ReturnsForbiddenEnvelope()
    {
        var token = await LoginAsUserAsync();
        SetBearerToken(token);

        var createResponse = await Client.PostAsJsonAsync("/api/clients", new
        {
            fullName = "João",
            documentNumber = "98765432100",
            email = "joao@challengebank.com"
        });

        var created = await ReadEnvelopeAsync<IdData>(createResponse);

        var patchResponse = await Client.PatchAsJsonAsync($"/api/clients/{created.Data!.Id}", new { name = "João Updated" });
        patchResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var envelope = await ReadEnvelopeAsync<object>(patchResponse);
        envelope.Status.Should().Be(403);
        envelope.Message.Should().Contain("permissão");
    }

    [Fact]
    public async Task CreateClient_WithoutToken_ReturnsUnauthorizedEnvelope()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.PostAsJsonAsync("/api/clients", new
        {
            fullName = "Ana",
            documentNumber = "11122233344",
            email = "ana@challengebank.com"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var envelope = await ReadEnvelopeAsync<object>(response);
        envelope.Status.Should().Be(401);
        envelope.Message.Should().NotBeNullOrWhiteSpace();
        envelope.Trace.Should().NotBeNullOrWhiteSpace();
    }
}
