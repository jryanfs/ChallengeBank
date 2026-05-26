using ChallengeBank.Api.Shared.Api;
using ChallengeBank.Api.Shared.Auth;
using ChallengeBank.Api.Shared.Extensions;
using ChallengeBank.Transactions.Application.DTOs;
using ChallengeBank.Transactions.Application.Transfers.Commands.CreateTransfer;
using ChallengeBank.Transactions.Application.Transfers.Queries.GetTransferById;
using ChallengeBank.Transactions.Application.Transfers.Queries.GetTransfersByUserId;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChallengeBank.Transactions.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Tags("Transferências")]
[Authorize(Roles = $"{Roles.User},{Roles.Admin}")]
public sealed class TransfersController(
    CreateTransferCommandHandler createTransferHandler,
    GetTransferByIdQueryHandler getTransferByIdHandler,
    GetTransfersByUserIdQueryHandler getTransfersByUserIdHandler) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiEnvelope), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiEnvelope), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiEnvelope), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiEnvelope), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiEnvelope), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Create([FromBody] CreateTransferRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateTransferCommand(
            request.SenderUserId ?? Guid.Empty,
            request.ReceiverUserId ?? Guid.Empty,
            request.Amount,
            request.Description);

        var result = await createTransferHandler.Handle(command, cancellationToken);

        return this.ToCreatedApiResult(
            result,
            nameof(GetById),
            dto => new { id = dto.Id },
            ApiMessages.TransferCreated,
            dto => dto);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiEnvelope), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await getTransferByIdHandler.Handle(new GetTransferByIdQuery(id), cancellationToken);
        return this.ToApiResult(result, ApiMessages.TransferRetrieved);
    }

    [HttpGet("user/{userId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiEnvelope), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiEnvelope), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiEnvelope), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiEnvelope), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiEnvelope), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var result = await getTransfersByUserIdHandler.Handle(new GetTransfersByUserIdQuery(userId), cancellationToken);
        return this.ToApiResult(result, ApiMessages.TransfersListed);
    }
}

public sealed record CreateTransferRequest(
    Guid? SenderUserId,
    Guid? ReceiverUserId,
    decimal Amount,
    string? Description);
