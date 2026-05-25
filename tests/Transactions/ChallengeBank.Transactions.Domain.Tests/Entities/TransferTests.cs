using ChallengeBank.BuildingBlocks.Domain.Exceptions;
using ChallengeBank.Transactions.Domain.Entities;
using ChallengeBank.Transactions.Domain.Enums;
using FluentAssertions;

namespace ChallengeBank.Transactions.Domain.Tests.Entities;

public sealed class TransferTests
{
    [Fact]
    public void Create_ShouldInitializePendingTransfer()
    {
        var sender = Guid.NewGuid();
        var receiver = Guid.NewGuid();
        var transfer = Transfer.Create(sender, receiver, 250.50m, "Payment");

        transfer.SenderUserId.Should().Be(sender);
        transfer.ReceiverUserId.Should().Be(receiver);
        transfer.Amount.Should().Be(250.50m);
        transfer.Description.Should().Be("Payment");
        transfer.Status.Should().Be(TransferStatus.Pending);
    }

    [Fact]
    public void Create_WithSameSenderAndReceiver_ShouldThrowDomainException()
    {
        var userId = Guid.NewGuid();
        var act = () => Transfer.Create(userId, userId, 100m);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Complete_ShouldSetCompletedStatus()
    {
        var transfer = Transfer.Create(Guid.NewGuid(), Guid.NewGuid(), 100m);
        transfer.Complete();
        transfer.Status.Should().Be(TransferStatus.Completed);
    }
}
