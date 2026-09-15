using Enterprise.TaskFlow.Domain.Enums;
using Enterprise.TaskFlow.Domain.Events;
using Enterprise.TaskFlow.Domain.Interfaces;
using MediatR;

namespace Enterprise.TaskFlow.Application.Commands.CompleteTask;

/// <summary>
/// Handles <see cref="CompleteTaskCommand"/> by updating the task status and
/// publishing a <see cref="TaskCompletedEvent"/> to the message broker.
/// </summary>
public sealed class CompleteTaskCommandHandler : IRequestHandler<CompleteTaskCommand, Unit>
{
    private readonly ITaskRepository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ICacheService _cache;

    public CompleteTaskCommandHandler(
        ITaskRepository repository,
        IEventPublisher eventPublisher,
        ICacheService cache)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _cache = cache;
    }

    public async Task<Unit> Handle(CompleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(request.TaskId, cancellationToken)
            ?? throw new KeyNotFoundException($"Task with ID '{request.TaskId}' was not found.");

        if (task.Status == TaskItemStatus.Completed)
            throw new InvalidOperationException($"Task '{request.TaskId}' is already completed.");

        // Update entity
        task.Status = TaskItemStatus.Completed;
        task.UpdatedAt = DateTime.UtcNow;

        // Persist — EF Core will enforce optimistic concurrency via RowVersion.
        // A DbUpdateConcurrencyException is thrown if the row was modified externally.
        await _repository.UpdateAsync(task, cancellationToken);

        // Invalidate the pending tasks cache
        await _cache.RemoveAsync("tasks:pending", cancellationToken);

        // Publish domain event to message broker
        var @event = new TaskCompletedEvent(task.Id, task.UpdatedAt.Value);
        await _eventPublisher.PublishTaskCompletedAsync(@event, cancellationToken);

        return Unit.Value;
    }
}
