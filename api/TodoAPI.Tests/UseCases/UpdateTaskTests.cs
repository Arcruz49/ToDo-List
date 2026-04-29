using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using TodoAPI.Application.DTOs;
using TodoAPI.Application.UseCases;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Enums;
using TodoAPI.Domain.Interfaces;
using TodoAPI.Domain.Interfaces.Persistence;

namespace TodoAPI.Tests.UseCases;

public class UpdateTaskTests
{
    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateTask _sut;

    public UpdateTaskTests()
    {
        _sut = new UpdateTask(_unitOfWork, _repository);
    }

    private TodoTask BuildTask(TodoTaskStatus status = TodoTaskStatus.Pending, DateTime? concludedAt = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            Title = "Original title",
            Description = "Original description",
            Color = "yellow",
            Status = status,
            CreatedAt = DateTime.UtcNow,
            ConcludedAt = concludedAt
        };

    [Fact]
    public async Task ExecuteAsync_UpdatesAllRequestFields()
    {
        var task = BuildTask();
        var request = new TaskEditRequest
        {
            Id = task.Id,
            Title = "Updated title",
            Description = "Updated description",
            Color = "blue",
            Status = TodoTaskStatus.InProgress
        };
        _repository.GetTask(task.Id).Returns(Task.FromResult(task));

        var result = await _sut.ExecuteAsync(request);

        Assert.Equal("Updated title", result.Title);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal("blue", result.Color);
        Assert.Equal(TodoTaskStatus.InProgress, result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStatusBecomesCompleted_SetsConcludedAt()
    {
        var task = BuildTask(TodoTaskStatus.InProgress);
        var request = new TaskEditRequest
        {
            Id = task.Id,
            Title = task.Title,
            Status = TodoTaskStatus.Completed
        };
        _repository.GetTask(task.Id).Returns(Task.FromResult(task));

        var before = DateTime.UtcNow;
        var result = await _sut.ExecuteAsync(request);
        var after = DateTime.UtcNow;

        Assert.NotNull(result.ConcludedAt);
        Assert.InRange(result.ConcludedAt.Value, before, after);
    }

    [Fact]
    public async Task ExecuteAsync_WhenAlreadyCompletedWithConcludedAt_DoesNotOverwriteConcludedAt()
    {
        var originalConcludedAt = new DateTime(2024, 3, 15, 10, 0, 0);
        var task = BuildTask(TodoTaskStatus.Completed, concludedAt: originalConcludedAt);
        var request = new TaskEditRequest
        {
            Id = task.Id,
            Title = "New title",
            Status = TodoTaskStatus.Completed
        };
        _repository.GetTask(task.Id).Returns(Task.FromResult(task));

        var result = await _sut.ExecuteAsync(request);

        Assert.Equal(originalConcludedAt, result.ConcludedAt);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStatusIsNotCompleted_DoesNotSetConcludedAt()
    {
        var task = BuildTask(TodoTaskStatus.Pending);
        var request = new TaskEditRequest
        {
            Id = task.Id,
            Title = task.Title,
            Status = TodoTaskStatus.InProgress
        };
        _repository.GetTask(task.Id).Returns(Task.FromResult(task));

        var result = await _sut.ExecuteAsync(request);

        Assert.Null(result.ConcludedAt);
    }

    [Fact]
    public async Task ExecuteAsync_CallsRepositoryUpdateAndSavesChanges()
    {
        var task = BuildTask();
        var request = new TaskEditRequest { Id = task.Id, Title = "Title", Status = TodoTaskStatus.Pending };
        _repository.GetTask(task.Id).Returns(Task.FromResult(task));

        await _sut.ExecuteAsync(request);

        _repository.Received(1).UpdateTask(Arg.Any<TodoTask>());
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WhenTaskNotFound_PropagatesException()
    {
        var id = Guid.NewGuid();
        _repository.GetTask(id).Throws(new KeyNotFoundException("Task not found"));

        var request = new TaskEditRequest { Id = id, Title = "Title", Status = TodoTaskStatus.Pending };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.ExecuteAsync(request));
    }

    [Fact]
    public async Task ExecuteAsync_WithNullOptionalFields_DefaultsToEmptyStrings()
    {
        var task = BuildTask();
        var request = new TaskEditRequest
        {
            Id = task.Id,
            Title = null,
            Description = null,
            Color = null,
            Status = TodoTaskStatus.Pending
        };
        _repository.GetTask(task.Id).Returns(Task.FromResult(task));

        var result = await _sut.ExecuteAsync(request);

        Assert.Equal("", result.Title);
        Assert.Equal("", result.Description);
        Assert.Equal("", result.Color);
    }
}
