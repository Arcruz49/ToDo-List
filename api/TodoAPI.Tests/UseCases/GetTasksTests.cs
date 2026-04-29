using NSubstitute;
using Xunit;
using TodoAPI.Application.UseCases;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Enums;
using TodoAPI.Domain.Interfaces;

namespace TodoAPI.Tests.UseCases;

public class GetTasksTests
{
    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly GetTasks _sut;

    public GetTasksTests()
    {
        _sut = new GetTasks(_repository);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleTasks_ReturnsMappedList()
    {
        var tasks = new List<TodoTask>
        {
            new() { Id = Guid.NewGuid(), Title = "Task A", Color = "yellow", CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Task B", Color = "green", CreatedAt = DateTime.UtcNow }
        };
        _repository.GetTasks().Returns(Task.FromResult(tasks));

        var result = await _sut.ExecuteAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Task A", result[0].Title);
        Assert.Equal("Task B", result[1].Title);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoTasks_ReturnsEmptyList()
    {
        _repository.GetTasks().Returns(Task.FromResult(new List<TodoTask>()));

        var result = await _sut.ExecuteAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task ExecuteAsync_MapsAllFieldsCorrectly()
    {
        var id = Guid.NewGuid();
        var concludedAt = DateTime.UtcNow;
        var tasks = new List<TodoTask>
        {
            new()
            {
                Id = id,
                Title = "Done task",
                Description = "Completed work",
                Status = TodoTaskStatus.Completed,
                Color = "pink",
                CreatedAt = new DateTime(2024, 6, 1),
                ConcludedAt = concludedAt
            }
        };
        _repository.GetTasks().Returns(Task.FromResult(tasks));

        var result = await _sut.ExecuteAsync();

        var item = result.Single();
        Assert.Equal(id, item.Id);
        Assert.Equal("Done task", item.Title);
        Assert.Equal("Completed work", item.Description);
        Assert.Equal(TodoTaskStatus.Completed, item.Status);
        Assert.Equal("pink", item.Color);
        Assert.Equal(concludedAt, item.ConcludedAt);
    }
}
