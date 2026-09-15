namespace Enterprise.TaskFlow.Domain.Entities;

/// <summary>
/// Core domain entity representing a task in the system.
/// Configured with optimistic concurrency via <see cref="RowVersion"/>.
/// </summary>
public class TaskItem
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Enums.TaskItemStatus Status { get; set; } = Enums.TaskItemStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Optimistic concurrency token.
    /// Mapped to SQL Server's <c>rowversion</c> / <c>timestamp</c> column.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
