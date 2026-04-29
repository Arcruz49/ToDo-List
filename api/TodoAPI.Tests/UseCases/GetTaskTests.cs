using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using TodoAPI.Application.UseCases;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Enums;
using TodoAPI.Domain.Interfaces;

namespace TodoAPI.Tests.UseCases;

public class GetTaskTests
{
    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly GetTask _sut;

    public GetTaskTests()
    {
        _sut = new GetTask(_repository);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingId_ReturnsMappedResponse()
    {
        var id = Guid.NewGuid();
        var task = new TodoTask
        {
            Id = id,
            Title = "Write tests",
            Description = "Add xUnit tests",
            Status = TodoTaskStatus.InProgress,
            Color = "blue",
            CreatedAt = new DateTime(2024, 1, 1)
        };
        _repository.GetTask(id).Returns(Task.FromResult(task));

        var result = await _sut.ExecuteAsync(id);

        Assert.Equal(id, result.Id);
        Assert.Equal("Write tests", result.Title);
        Assert.Equal("Add xUnit tests", result.Description);
        Assert.Equal(TodoTaskStatus.InProgress, result.Status);
        Assert.Equal("blue", result.Color);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryThrows_PropagatesException()
    {
        var id = Guid.NewGuid();
        _repository.GetTask(id).Throws(new KeyNotFoundException("Task not found"));

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.ExecuteAsync(id));
    }
}
