using MediatR;

namespace Enterprise.TaskFlow.Application.Commands.CompleteTask;

/// <summary>
/// Command to mark a task as completed.
/// Triggers a <see cref="Domain.Events.TaskCompletedEvent"/> via the event publisher.
/// </summary>
public sealed record CompleteTaskCommand(Guid TaskId) : IRequest<Unit>;
