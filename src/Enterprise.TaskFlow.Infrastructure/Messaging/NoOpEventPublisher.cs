using Enterprise.TaskFlow.Domain.Events;
using Enterprise.TaskFlow.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Enterprise.TaskFlow.Infrastructure.Messaging;

/// <summary>
/// No-op fallback implementation of <see cref="IEventPublisher"/>
/// used when RabbitMQ is unavailable (e.g., local development without Docker).
/// </summary>
public class NoOpEventPublisher : IEventPublisher
{
    private readonly ILogger _logger;

    public NoOpEventPublisher(ILogger logger)
    {
        _logger = logger;
    }

    public Task PublishTaskCompletedAsync(TaskCompletedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(
            "NoOpEventPublisher: TaskCompletedEvent for TaskId={TaskId} was NOT published (RabbitMQ unavailable).",
            @event.TaskId);

        return Task.CompletedTask;
    }
}
