namespace Enterprise.TaskFlow.Domain.Enums;

/// <summary>
/// Represents the lifecycle status of a <see cref="Entities.TaskItem"/>.
/// </summary>
public enum TaskItemStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}
