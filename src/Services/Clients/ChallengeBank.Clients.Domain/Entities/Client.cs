using ChallengeBank.BuildingBlocks.Domain.Abstractions;
using ChallengeBank.BuildingBlocks.Domain.Exceptions;
using ChallengeBank.Clients.Domain.Enums;

namespace ChallengeBank.Clients.Domain.Entities;

public sealed class Client : AggregateRoot<Guid>, IAuditableEntity
{
    private Client() { }

    public string FullName { get; private set; } = string.Empty;

    public string DocumentNumber { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public ClientStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    public static Client Create(string fullName, string documentNumber, string email)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Client name is required.");

        if (string.IsNullOrWhiteSpace(documentNumber))
            throw new DomainException("Document number is required.");

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");

        return new Client
        {
            Id = Guid.NewGuid(),
            FullName = fullName.Trim(),
            DocumentNumber = documentNumber.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Status = ClientStatus.Active,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Deactivate()
    {
        if (Status == ClientStatus.Inactive)
            throw new DomainException("Client is already inactive.");

        Status = ClientStatus.Inactive;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
