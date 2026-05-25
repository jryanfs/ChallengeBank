using ChallengeBank.Clients.Application.Clients.Commands.CreateClient;
using ChallengeBank.Clients.Application.Clients.Queries.GetClientById;
using Microsoft.AspNetCore.Mvc;

namespace ChallengeBank.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Clientes")]
public sealed class ClientsController(
    CreateClientCommandHandler createClientHandler,
    GetClientByIdQueryHandler getClientByIdHandler) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateClientRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateClientCommand(request.FullName, request.DocumentNumber, request.Email);
        var result = await createClientHandler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return MapError(result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await getClientByIdHandler.Handle(new GetClientByIdQuery(id), cancellationToken);

        if (result.IsFailure)
            return MapError(result.Error);

        return Ok(result.Value);
    }

    private IActionResult MapError(ChallengeBank.BuildingBlocks.Application.Common.Error error) =>
        error.Code switch
        {
            var code when code.StartsWith("Client.NotFound", StringComparison.Ordinal) => NotFound(error),
            var code when code.StartsWith("Client.DocumentExists", StringComparison.Ordinal) => Conflict(error),
            _ => BadRequest(error)
        };
}

public sealed record CreateClientRequest(string FullName, string DocumentNumber, string Email);
