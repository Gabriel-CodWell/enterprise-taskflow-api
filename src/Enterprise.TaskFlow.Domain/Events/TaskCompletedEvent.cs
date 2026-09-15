namespace Enterprise.TaskFlow.Domain.Events;

/// <summary>
/// Domain event raised when a task is marked as completed.
/// Published to the message broker for downstream consumers.
/// </summary>
public sealed record TaskCompletedEvent(Guid TaskId, DateTime CompletedAt);
