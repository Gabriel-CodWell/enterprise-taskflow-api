using Enterprise.TaskFlow.Domain.Entities;
using MediatR;

namespace Enterprise.TaskFlow.Application.Queries.GetPendingTasks;

/// <summary>
/// Query to retrieve all tasks with Pending status.
/// Results are served from cache when available.
/// </summary>
public sealed record GetPendingTasksQuery : IRequest<IEnumerable<TaskItem>>;
