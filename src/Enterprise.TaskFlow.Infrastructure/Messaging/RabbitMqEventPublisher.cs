using System.Text;
using System.Text.Json;
using Enterprise.TaskFlow.Domain.Events;
using Enterprise.TaskFlow.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Enterprise.TaskFlow.Infrastructure.Messaging;

/// <summary>
/// RabbitMQ-backed implementation of <see cref="IEventPublisher"/>.
/// Publishes serialized domain events to a fanout exchange.
/// </summary>
public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private const string ExchangeName = "taskflow.events";
    private const string RoutingKey = "task.completed";

    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqEventPublisher> _logger;

    public RabbitMqEventPublisher(IConfiguration configuration, ILogger<RabbitMqEventPublisher> logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMq:HostName"] ?? "localhost",
            UserName = configuration["RabbitMq:UserName"] ?? "guest",
            Password = configuration["RabbitMq:Password"] ?? "guest",
            Port = int.TryParse(configuration["RabbitMq:Port"], out var port) ? port : 5672
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare the fanout exchange (idempotent)
        _channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false);

        _logger.LogInformation("RabbitMQ connection established. Exchange '{Exchange}' declared.", ExchangeName);
    }

    public Task PublishTaskCompletedAsync(TaskCompletedEvent @event, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel.CreateBasicProperties();
        properties.ContentType = "application/json";
        properties.DeliveryMode = 2; // persistent
        properties.MessageId = Guid.NewGuid().ToString();
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        _channel.BasicPublish(
            exchange: ExchangeName,
            routingKey: RoutingKey,
            mandatory: false,
            basicProperties: properties,
            body: body);

        _logger.LogInformation(
            "Published TaskCompletedEvent for TaskId={TaskId} to exchange '{Exchange}'.",
            @event.TaskId,
            ExchangeName);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        GC.SuppressFinalize(this);
    }
}
