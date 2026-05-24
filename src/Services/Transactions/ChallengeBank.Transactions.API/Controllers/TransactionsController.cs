using ChallengeBank.Transactions.Application.Transactions.Commands.CreateTransaction;
using ChallengeBank.Transactions.Application.Transactions.Queries.GetTransactionById;
using ChallengeBank.Transactions.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace ChallengeBank.Transactions.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TransactionsController(
    CreateTransactionCommandHandler createTransactionHandler,
    GetTransactionByIdQueryHandler getTransactionByIdHandler) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateTransactionCommand(
            request.ClientId,
            request.Amount,
            request.Type,
            request.Description);

        var result = await createTransactionHandler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return MapError(result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await getTransactionByIdHandler.Handle(new GetTransactionByIdQuery(id), cancellationToken);

        if (result.IsFailure)
            return MapError(result.Error);

        return Ok(result.Value);
    }

    private IActionResult MapError(ChallengeBank.BuildingBlocks.Application.Common.Error error) =>
        error.Code switch
        {
            var code when code.StartsWith("Transaction.NotFound", StringComparison.Ordinal) => NotFound(error),
            _ => BadRequest(error)
        };
}

public sealed record CreateTransactionRequest(
    Guid ClientId,
    decimal Amount,
    TransactionType Type,
    string? Description);
