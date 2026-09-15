using Enterprise.TaskFlow.Application.Commands.CompleteTask;
using Enterprise.TaskFlow.Application.Commands.CreateTask;
using Enterprise.TaskFlow.Application.Queries.GetPendingTasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.TaskFlow.API.Controllers;

/// <summary>
/// API endpoints for managing tasks.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves all tasks with Pending status.
    /// Results are served from Redis cache when available (5-min TTL).
    /// </summary>
    [HttpGet("pending")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingTasks(CancellationToken cancellationToken)
    {
        var tasks = await _mediator.Send(new GetPendingTasksQuery(), cancellationToken);
        return Ok(tasks);
    }

    /// <summary>
    /// Creates a new task.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskCommand command, CancellationToken cancellationToken)
    {
        var taskId = await _mediator.Send(command, cancellationToken);
        return Created($"/api/tasks/{taskId}", new { id = taskId });
    }

    /// <summary>
    /// Marks a task as completed.
    /// Publishes a TaskCompletedEvent to RabbitMQ and invalidates the pending tasks cache.
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CompleteTask(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new CompleteTaskCommand(id), cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = $"Task with ID '{id}' was not found." });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
