using ChallengeBank.API.Controllers;
using FluentValidation;

namespace ChallengeBank.API.Validators;

public sealed class UpdateClientRequestValidator : AbstractValidator<UpdateClientRequest>
{
    public UpdateClientRequestValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200)
            .WithMessage("O nome deve ter no máximo 200 caracteres.")
            .When(x => x.Name is not null);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("O e-mail informado é inválido.")
            .MaximumLength(256).WithMessage("O e-mail deve ter no máximo 256 caracteres.")
            .When(x => x.Email is not null);

        When(x => x.BankingDetails is not null, () =>
        {
            RuleFor(x => x.BankingDetails!.Agency)
                .NotEmpty().WithMessage("A agência bancária é obrigatória.")
                .MaximumLength(20).WithMessage("A agência deve ter no máximo 20 caracteres.");

            RuleFor(x => x.BankingDetails!.AccountNumber)
                .NotEmpty().WithMessage("O número da conta bancária é obrigatório.")
                .MaximumLength(30).WithMessage("O número da conta deve ter no máximo 30 caracteres.");
        });
    }
}
