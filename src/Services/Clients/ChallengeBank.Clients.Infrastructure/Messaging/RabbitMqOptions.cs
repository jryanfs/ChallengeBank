namespace ChallengeBank.Clients.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public string HostName { get; init; } = "localhost";

    public int Port { get; init; } = 5672;

    public string UserName { get; init; } = "guest";

    public string Password { get; init; } = "guest";

    public string Exchange { get; init; } = "challengebank.events";

    public string RoutingKeyBankingDetailsUpdated { get; init; } = "clients.bankingdetails.updated";
}

