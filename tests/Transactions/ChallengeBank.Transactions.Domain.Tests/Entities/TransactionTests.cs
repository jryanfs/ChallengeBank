using ChallengeBank.BuildingBlocks.Domain.Exceptions;
using ChallengeBank.Transactions.Domain.Entities;
using ChallengeBank.Transactions.Domain.Enums;
using FluentAssertions;

namespace ChallengeBank.Transactions.Domain.Tests.Entities;

public sealed class TransactionTests
{
    [Fact]
    public void Create_ShouldInitializePendingTransaction()
    {
        var clientId = Guid.NewGuid();
        var transaction = Transaction.Create(clientId, 150.75m, TransactionType.Credit, "Deposit");

        transaction.Id.Should().NotBeEmpty();
        transaction.ClientId.Should().Be(clientId);
        transaction.Amount.Should().Be(150.75m);
        transaction.Type.Should().Be(TransactionType.Credit);
        transaction.Status.Should().Be(TransactionStatus.Pending);
        transaction.Description.Should().Be("Deposit");
    }

    [Fact]
    public void Create_WithInvalidAmount_ShouldThrowDomainException()
    {
        var act = () => Transaction.Create(Guid.NewGuid(), 0, TransactionType.Debit);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Complete_ShouldSetCompletedStatus()
    {
        var transaction = Transaction.Create(Guid.NewGuid(), 100m, TransactionType.Debit);

        transaction.Complete();

        transaction.Status.Should().Be(TransactionStatus.Completed);
    }
}
