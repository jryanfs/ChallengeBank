using ChallengeBank.BuildingBlocks.Domain.Abstractions;
using ChallengeBank.BuildingBlocks.Domain.Exceptions;
using ChallengeBank.Clients.Domain.Enums;
using ChallengeBank.Clients.Domain.ValueObjects;

namespace ChallengeBank.Clients.Domain.Entities;

public sealed class Client : AggregateRoot<Guid>, IAuditableEntity
{
    private Client() { }

    public string FullName { get; private set; } = string.Empty;

    public string DocumentNumber { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public Address? Address { get; private set; }

    public BankingDetails? BankingDetails { get; private set; }

    public ClientStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    public static Client Create(
        string fullName,
        string documentNumber,
        string email,
        Address? address = null,
        BankingDetails? bankingDetails = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("O nome do cliente é obrigatório.");

        if (string.IsNullOrWhiteSpace(documentNumber))
            throw new DomainException("O número do documento é obrigatório.");

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("O e-mail é obrigatório.");

        return new Client
        {
            Id = Guid.NewGuid(),
            FullName = fullName.Trim(),
            DocumentNumber = documentNumber.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Address = address,
            BankingDetails = bankingDetails,
            Status = ClientStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void UpdatePartial(
        string? name,
        string? email,
        Address? address,
        BankingDetails? bankingDetails,
        bool updateAddress,
        bool updateBankingDetails)
    {
        if (name is not null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("O nome do cliente não pode ser vazio.");

            FullName = name.Trim();
        }

        if (email is not null)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new DomainException("O e-mail não pode ser vazio.");

            Email = email.Trim().ToLowerInvariant();
        }

        if (updateAddress)
            Address = address;

        if (updateBankingDetails)
            BankingDetails = bankingDetails;

        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (Status == ClientStatus.Inactive)
            throw new DomainException("O cliente já está inativo.");

        Status = ClientStatus.Inactive;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
