using Enterprise.TaskFlow.Domain.Events;

namespace Enterprise.TaskFlow.Domain.Interfaces;

/// <summary>
/// Abstraction for publishing domain events to a message broker.
/// </summary>
public interface IEventPublisher
{
    Task PublishTaskCompletedAsync(TaskCompletedEvent @event, CancellationToken cancellationToken = default);
}
