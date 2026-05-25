using ChallengeBank.API.Controllers;
using FluentValidation;

namespace ChallengeBank.API.Validators;

public sealed class CreateClientRequestValidator : AbstractValidator<CreateClientRequest>
{
    public CreateClientRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("O nome completo é obrigatório.")
            .MaximumLength(200).WithMessage("O nome completo deve ter no máximo 200 caracteres.");

        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage("O número do documento é obrigatório.")
            .MaximumLength(20).WithMessage("O número do documento deve ter no máximo 20 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("O e-mail informado é inválido.")
            .MaximumLength(256).WithMessage("O e-mail deve ter no máximo 256 caracteres.");

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
