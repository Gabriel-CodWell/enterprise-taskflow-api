using Enterprise.TaskFlow.Domain.Entities;
using Enterprise.TaskFlow.Domain.Interfaces;
using MediatR;

namespace Enterprise.TaskFlow.Application.Commands.CreateTask;

public sealed class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Guid>
{
    private readonly ITaskRepository _repository;
    private readonly ICacheService _cache;

    public CreateTaskCommandHandler(ITaskRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<Guid> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(task, cancellationToken);

        // Invalidate the pending tasks cache
        await _cache.RemoveAsync("tasks:pending", cancellationToken);

        return task.Id;
    }
}
