using ChallengeBank.BuildingBlocks.Domain.Exceptions;
using ChallengeBank.Clients.Domain.Entities;
using ChallengeBank.Clients.Domain.Enums;
using FluentAssertions;

namespace ChallengeBank.Clients.Domain.Tests.Entities;

public sealed class ClientTests
{
    [Fact]
    public void Create_ShouldInitializeActiveClient()
    {
        var client = Client.Create("Maria Silva", "12345678901", "maria@email.com");

        client.Id.Should().NotBeEmpty();
        client.FullName.Should().Be("Maria Silva");
        client.DocumentNumber.Should().Be("12345678901");
        client.Email.Should().Be("maria@email.com");
        client.Status.Should().Be(ClientStatus.Active);
        client.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowDomainException()
    {
        var act = () => Client.Create("", "12345678901", "maria@email.com");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Deactivate_ShouldSetInactiveStatus()
    {
        var client = Client.Create("Maria Silva", "12345678901", "maria@email.com");

        client.Deactivate();

        client.Status.Should().Be(ClientStatus.Inactive);
        client.UpdatedAtUtc.Should().NotBeNull();
    }
}
