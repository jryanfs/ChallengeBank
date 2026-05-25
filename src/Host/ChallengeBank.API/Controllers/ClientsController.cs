using ChallengeBank.API.Api;
using ChallengeBank.API.Auth;
using ChallengeBank.API.Extensions;
using ChallengeBank.Clients.Application.Clients.Commands.CreateClient;
using ChallengeBank.Clients.Application.Clients.Commands.UpdateClient;
using ChallengeBank.Clients.Application.Clients.Queries.GetClientById;
using ChallengeBank.Clients.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChallengeBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Clientes")]
[Authorize(Roles = $"{Roles.User},{Roles.Admin}")]
public sealed class ClientsController(
    CreateClientCommandHandler createClientHandler,
    UpdateClientCommandHandler updateClientHandler,
    GetClientByIdQueryHandler getClientByIdHandler) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiEnvelope), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiEnvelope), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiEnvelope), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateClientRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateClientCommand(
            request.FullName,
            request.DocumentNumber,
            request.Email,
            request.Address,
            request.BankingDetails);

        var result = await createClientHandler.Handle(command, cancellationToken);

        return this.ToCreatedApiResult(
            result,
            nameof(GetById),
            id => new { id },
            ApiMessages.ClientCreated,
            id => new { id });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiEnvelope), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await getClientByIdHandler.Handle(new GetClientByIdQuery(id), cancellationToken);
        return this.ToApiResult(result, ApiMessages.ClientRetrieved);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiEnvelope), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiEnvelope), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiEnvelope), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePartial(Guid id, [FromBody] UpdateClientRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateClientCommand(id, request.Name, request.Email, request.Address, request.BankingDetails);
        var result = await updateClientHandler.Handle(command, cancellationToken);
        return this.ToApiResult(result, ApiMessages.ClientUpdated);
    }
}

public sealed record CreateClientRequest(
    string FullName,
    string DocumentNumber,
    string Email,
    AddressDto? Address = null,
    BankingDetailsDto? BankingDetails = null);

public sealed record UpdateClientRequest(
    string? Name,
    string? Email,
    AddressDto? Address,
    BankingDetailsDto? BankingDetails);
