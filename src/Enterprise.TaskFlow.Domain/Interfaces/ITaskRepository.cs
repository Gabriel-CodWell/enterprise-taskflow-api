using Enterprise.TaskFlow.Domain.Entities;

namespace Enterprise.TaskFlow.Domain.Interfaces;

/// <summary>
/// Repository contract for <see cref="TaskItem"/> persistence operations.
/// </summary>
public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<TaskItem>> GetPendingTasksAsync(CancellationToken cancellationToken = default);

    Task AddAsync(TaskItem taskItem, CancellationToken cancellationToken = default);

    Task UpdateAsync(TaskItem taskItem, CancellationToken cancellationToken = default);
}
