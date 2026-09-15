using Enterprise.TaskFlow.Application.Commands.CreateTask;
using Enterprise.TaskFlow.Domain.Interfaces;
using Enterprise.TaskFlow.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Enterprise.TaskFlow.UnitTests;

public class CreateTaskCommandHandlerTests
{
    private readonly Mock<ITaskRepository> _repositoryMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly CreateTaskCommandHandler _handler;

    public CreateTaskCommandHandlerTests()
    {
        _repositoryMock = new Mock<ITaskRepository>();
        _cacheMock = new Mock<ICacheService>();
        _handler = new CreateTaskCommandHandler(_repositoryMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateTaskAndInvalidateCache()
    {
        // Arrange
        var command = new CreateTaskCommand("Test Task", "Test Description");
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _repositoryMock.Verify(x => x.AddAsync(It.Is<TaskItem>(t => t.Title == command.Title), It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(x => x.RemoveAsync("tasks:pending", It.IsAny<CancellationToken>()), Times.Once);
    }
}
