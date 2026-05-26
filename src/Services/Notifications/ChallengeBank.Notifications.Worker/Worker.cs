using System.Text;
using System.Text.Json;
using ChallengeBank.Contracts.Events;
using ChallengeBank.Contracts.Notifications;
using ChallengeBank.Notifications.Worker.Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ChallengeBank.Notifications.Worker;

public sealed class Worker(
    ILogger<Worker> logger,
    IOptions<RabbitMqOptions> options,
    INotificationService notificationService) : BackgroundService
{
    private readonly RabbitMqOptions _options = options.Value;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        DictionaryKeyPolicy = null
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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
        channel.QueueDeclare(_options.QueueBankingDetailsUpdated, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(_options.QueueBankingDetailsUpdated, _options.Exchange, _options.RoutingKeyBankingDetailsUpdated);

        channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var ev = JsonSerializer.Deserialize<ClientBankingDetailsUpdatedEvent>(json, JsonOptions);

                if (ev is null)
                {
                    channel.BasicAck(ea.DeliveryTag, multiple: false);
                    return;
                }

                await notificationService.SendBankingDetailsUpdatedAsync(ev, stoppingToken);
                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao processar mensagem RabbitMQ. DeliveryTag={DeliveryTag}", ea.DeliveryTag);
                // Requeue simples (para demo). Em produção: DLQ e política mais robusta.
                channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        var consumerTag = channel.BasicConsume(_options.QueueBankingDetailsUpdated, autoAck: false, consumer: consumer);
        logger.LogInformation("Worker RabbitMQ iniciado. Queue={Queue} ConsumerTag={Tag}", _options.QueueBankingDetailsUpdated, consumerTag);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // shutdown
        }
    }
}
