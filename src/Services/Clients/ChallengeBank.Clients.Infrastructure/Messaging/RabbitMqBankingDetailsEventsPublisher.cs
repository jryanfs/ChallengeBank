using System.Text;
using System.Text.Json;
using ChallengeBank.Clients.Application.Abstractions;
using ChallengeBank.Contracts.Events;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ChallengeBank.Clients.Infrastructure.Messaging;

public sealed class RabbitMqBankingDetailsEventsPublisher(IOptions<RabbitMqOptions> options) : IBankingDetailsEventsPublisher
{
    private readonly RabbitMqOptions _options = options.Value;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        DictionaryKeyPolicy = null
    };

    public Task PublishUpdatedAsync(ClientBankingDetailsUpdatedEvent @event, CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            DispatchConsumersAsync = true
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(_options.Exchange, ExchangeType.Topic, durable: true, autoDelete: false);

        var payload = JsonSerializer.Serialize(@event, JsonOptions);
        var body = Encoding.UTF8.GetBytes(payload);

        var props = channel.CreateBasicProperties();
        props.ContentType = "application/json";
        props.DeliveryMode = 2; // persistent

        channel.BasicPublish(
            exchange: _options.Exchange,
            routingKey: _options.RoutingKeyBankingDetailsUpdated,
            basicProperties: props,
            body: body);

        return Task.CompletedTask;
    }
}

