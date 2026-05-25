using System.Net;
using System.Net.Http.Json;
using ChallengeBank.API.IntegrationTests.Infrastructure;
using ChallengeBank.Transactions.Application.DTOs;
using FluentAssertions;

namespace ChallengeBank.API.IntegrationTests.Flows;

public sealed class TransferFlowIntegrationTests(ChallengeBankWebApplicationFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateTransfer_AndGetById_AsAdmin_ReturnsCompletedTransferEnvelope()
    {
        var token = await LoginAsAdminAsync();
        SetBearerToken(token);

        var senderId = await CreateClientAsync("11111111111", "sender@challengebank.com", "Sender");
        var receiverId = await CreateClientAsync("22222222222", "receiver@challengebank.com", "Receiver");

        var createTransferResponse = await Client.PostAsJsonAsync("/api/transfers", new
        {
            senderUserId = senderId,
            receiverUserId = receiverId,
            amount = 250.50m,
            description = "Integration test transfer"
        });

        createTransferResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdEnvelope = await ReadEnvelopeAsync<CreateTransferData>(createTransferResponse);
        createdEnvelope.Message.Should().Contain("sucesso");
        createdEnvelope.Data!.Status.Should().Be(2);

        var getResponse = await Client.GetAsync($"/api/transfers/{createdEnvelope.Data.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getEnvelope = await ReadEnvelopeAsync<TransferDto>(getResponse);
        getEnvelope.Data!.Amount.Should().Be(250.50m);
        getEnvelope.Data.SenderUserId.Should().Be(senderId);
    }

    [Fact]
    public async Task GetTransfer_WhenIdDoesNotExist_ReturnsNotFoundEnvelopeWithMessage()
    {
        var token = await LoginAsAdminAsync();
        SetBearerToken(token);

        var response = await Client.GetAsync($"/api/transfers/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var envelope = await ReadEnvelopeAsync<object>(response);
        envelope.Status.Should().Be(404);
        envelope.Message.Should().NotBeNullOrWhiteSpace();
        envelope.Message.Should().Contain("Transferência");
        envelope.Message.Should().Contain("não foi encontrada");
        envelope.Trace.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task CreateTransfer_WithNullSenderUserId_ReturnsBadRequestInPortuguese()
    {
        var token = await LoginAsAdminAsync();
        SetBearerToken(token);

        var clientId = await CreateClientAsync("88877766655", "receiver@challengebank.com", "Receiver");

        var response = await Client.PostAsJsonAsync("/api/transfers", new
        {
            senderUserId = (Guid?)null,
            receiverUserId = clientId,
            amount = 100m,
            description = "Sem remetente"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var envelope = await ReadEnvelopeAsync<object>(response);
        envelope.Message.Should().Contain("remetente");
    }

    [Fact]
    public async Task CreateTransfer_WithNonExistentSender_ReturnsNotFoundInPortuguese()
    {
        var token = await LoginAsAdminAsync();
        SetBearerToken(token);

        var receiverId = await CreateClientAsync("77766655544", "onlyreceiver@challengebank.com", "Only Receiver");
        var missingSenderId = Guid.NewGuid();

        var response = await Client.PostAsJsonAsync("/api/transfers", new
        {
            senderUserId = missingSenderId,
            receiverUserId = receiverId,
            amount = 100m,
            description = "Remetente inexistente"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var envelope = await ReadEnvelopeAsync<object>(response);
        envelope.Message.Should().Contain("remetente");
        envelope.Message.Should().Contain(missingSenderId.ToString());
    }

    [Fact]
    public async Task CreateTransfer_WithNonExistentReceiver_ReturnsNotFoundInPortuguese()
    {
        var token = await LoginAsAdminAsync();
        SetBearerToken(token);

        var senderId = await CreateClientAsync("66655544433", "onlysender@challengebank.com", "Only Sender");
        var missingReceiverId = Guid.NewGuid();

        var response = await Client.PostAsJsonAsync("/api/transfers", new
        {
            senderUserId = senderId,
            receiverUserId = missingReceiverId,
            amount = 100m,
            description = "Destinatário inexistente"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var envelope = await ReadEnvelopeAsync<object>(response);
        envelope.Message.Should().Contain("destinatário");
        envelope.Message.Should().Contain(missingReceiverId.ToString());
    }

    [Fact]
    public async Task ListTransfersByUser_WhenUserDoesNotExist_ReturnsNotFoundInPortuguese()
    {
        var token = await LoginAsAdminAsync();
        SetBearerToken(token);

        var missingUserId = Guid.NewGuid();
        var response = await Client.GetAsync($"/api/transfers/user/{missingUserId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var envelope = await ReadEnvelopeAsync<object>(response);
        envelope.Message.Should().Contain("não foi encontrado");
        envelope.Message.Should().Contain(missingUserId.ToString());
    }

    [Fact]
    public async Task CreateTransfer_WithSameSenderAndReceiver_ReturnsBadRequestInPortuguese()
    {
        var token = await LoginAsAdminAsync();
        SetBearerToken(token);

        var clientId = await CreateClientAsync("99988877766", "same@challengebank.com", "Same User");

        var response = await Client.PostAsJsonAsync("/api/transfers", new
        {
            senderUserId = clientId,
            receiverUserId = clientId,
            amount = 100m,
            description = "Inválido"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var envelope = await ReadEnvelopeAsync<object>(response);
        envelope.Message.Should().Contain("remetente e o destinatário");
        envelope.Message.Should().Contain("diferentes");
    }

    [Fact]
    public async Task ListTransfersByUser_AsUser_ReturnsForbidden_Admin_ReturnsOkEnvelope()
    {
        var adminToken = await LoginAsAdminAsync();
        SetBearerToken(adminToken);

        var clientId = await CreateClientAsync("33333333333", "list@challengebank.com", "List User");

        var userToken = await LoginAsUserAsync();
        SetBearerToken(userToken);

        var forbiddenResponse = await Client.GetAsync($"/api/transfers/user/{clientId}");
        forbiddenResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var forbiddenEnvelope = await ReadEnvelopeAsync<object>(forbiddenResponse);
        forbiddenEnvelope.Status.Should().Be(403);
        forbiddenEnvelope.Message.Should().NotBeNullOrWhiteSpace();

        SetBearerToken(adminToken);
        var okResponse = await Client.GetAsync($"/api/transfers/user/{clientId}");
        okResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var okEnvelope = await ReadEnvelopeAsync<List<TransferDto>>(okResponse);
        okEnvelope.Message.Should().Contain("listadas");
    }

    private async Task<Guid> CreateClientAsync(string document, string email, string name)
    {
        var response = await Client.PostAsJsonAsync("/api/clients", new
        {
            fullName = name,
            documentNumber = document,
            email
        });

        response.EnsureSuccessStatusCode();
        var envelope = await ReadEnvelopeAsync<IdData>(response);
        return envelope.Data!.Id;
    }
}
