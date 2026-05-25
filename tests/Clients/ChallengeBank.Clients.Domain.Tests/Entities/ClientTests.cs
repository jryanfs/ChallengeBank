using ChallengeBank.BuildingBlocks.Domain.Exceptions;
using ChallengeBank.Clients.Domain.Entities;
using ChallengeBank.Clients.Domain.Enums;
using ChallengeBank.Clients.Domain.ValueObjects;
using FluentAssertions;

namespace ChallengeBank.Clients.Domain.Tests.Entities;

public sealed class ClientTests
{
    [Fact]
    public void Create_ShouldInitializeActiveClientWithBankingDetails()
    {
        var banking = BankingDetails.Create("0001", "12345-6");
        var client = Client.Create("Maria Silva", "12345678901", "maria@email.com", bankingDetails: banking);

        client.BankingDetails.Should().NotBeNull();
        client.BankingDetails!.Agency.Should().Be("0001");
        client.BankingDetails.AccountNumber.Should().Be("12345-6");
        client.Status.Should().Be(ClientStatus.Active);
    }

    [Fact]
    public void UpdatePartial_ShouldUpdateNameAndBanking()
    {
        var client = Client.Create("Maria Silva", "12345678901", "maria@email.com");
        var banking = BankingDetails.Create("0002", "99999-9");

        client.UpdatePartial("Maria Santos", null, null, banking, updateAddress: false, updateBankingDetails: true);

        client.FullName.Should().Be("Maria Santos");
        client.BankingDetails!.Agency.Should().Be("0002");
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowDomainException()
    {
        var act = () => Client.Create("", "12345678901", "maria@email.com");
        act.Should().Throw<DomainException>();
    }
}
