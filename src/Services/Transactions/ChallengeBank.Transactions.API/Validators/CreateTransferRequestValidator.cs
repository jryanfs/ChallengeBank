using ChallengeBank.Transactions.API.Controllers;
using FluentValidation;

namespace ChallengeBank.Transactions.API.Validators;

public sealed class CreateTransferRequestValidator : AbstractValidator<CreateTransferRequest>
{
    public CreateTransferRequestValidator()
    {
        RuleFor(x => x.SenderUserId)
            .Must(id => id.HasValue && id.Value != Guid.Empty)
            .WithMessage("O identificador do remetente é obrigatório.");

        RuleFor(x => x.ReceiverUserId)
            .Must(id => id.HasValue && id.Value != Guid.Empty)
            .WithMessage("O identificador do destinatário é obrigatório.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("O valor da transferência deve ser maior que zero.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("A descrição deve ter no máximo 500 caracteres.")
            .When(x => x.Description is not null);

        RuleFor(x => x)
            .Must(x => x.SenderUserId.HasValue && x.ReceiverUserId.HasValue
                       && x.SenderUserId.Value != x.ReceiverUserId.Value)
            .WithMessage("O remetente e o destinatário devem ser usuários diferentes.")
            .When(x => x.SenderUserId is { } s && s != Guid.Empty
                       && x.ReceiverUserId is { } r && r != Guid.Empty);
    }
}
