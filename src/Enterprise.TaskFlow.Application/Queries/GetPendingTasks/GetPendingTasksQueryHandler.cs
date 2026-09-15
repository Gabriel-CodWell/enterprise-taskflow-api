using Enterprise.TaskFlow.Domain.Entities;
using Enterprise.TaskFlow.Domain.Interfaces;
using MediatR;

namespace Enterprise.TaskFlow.Application.Queries.GetPendingTasks;

/// <summary>
/// Handles <see cref="GetPendingTasksQuery"/> by checking the cache first,
/// then falling back to the repository and populating the cache.
/// </summary>
public sealed class GetPendingTasksQueryHandler
    : IRequestHandler<GetPendingTasksQuery, IEnumerable<TaskItem>>
{
    private const string CacheKey = "tasks:pending";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly ITaskRepository _repository;
    private readonly ICacheService _cache;

    public GetPendingTasksQueryHandler(ITaskRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<IEnumerable<TaskItem>> Handle(
        GetPendingTasksQuery request,
        CancellationToken cancellationToken)
    {
        // Try cache first
        var cached = await _cache.GetAsync<IEnumerable<TaskItem>>(CacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        // Cache miss — fetch from database
        var tasks = await _repository.GetPendingTasksAsync(cancellationToken);

        // Populate cache for subsequent calls
        await _cache.SetAsync(CacheKey, tasks, CacheDuration, cancellationToken);

        return tasks;
    }
}
