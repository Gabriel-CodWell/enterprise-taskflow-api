using MediatR;

namespace Enterprise.TaskFlow.Application.Commands.CreateTask;

public record CreateTaskCommand(string Title, string? Description) : IRequest<Guid>;
