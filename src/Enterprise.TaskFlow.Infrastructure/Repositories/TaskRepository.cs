using Enterprise.TaskFlow.Domain.Entities;
using Enterprise.TaskFlow.Domain.Enums;
using Enterprise.TaskFlow.Domain.Interfaces;
using Enterprise.TaskFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Enterprise.TaskFlow.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ITaskRepository"/>.
/// </summary>
public class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext _context;

    public TaskRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TaskItems.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<TaskItem>> GetPendingTasksAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TaskItems
            .Where(t => t.Status == TaskItemStatus.Pending)
            .OrderByDescending(t => t.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TaskItem taskItem, CancellationToken cancellationToken = default)
    {
        await _context.TaskItems.AddAsync(taskItem, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TaskItem taskItem, CancellationToken cancellationToken = default)
    {
        _context.TaskItems.Update(taskItem);
        await _context.SaveChangesAsync(cancellationToken);
        // DbUpdateConcurrencyException is thrown by EF Core if RowVersion doesn't match
    }
}
