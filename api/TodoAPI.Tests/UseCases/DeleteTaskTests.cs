using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using TodoAPI.Application.UseCases;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Interfaces;
using TodoAPI.Domain.Interfaces.Persistence;

namespace TodoAPI.Tests.UseCases;

public class DeleteTaskTests
{
    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteTask _sut;

    public DeleteTaskTests()
    {
        _sut = new DeleteTask(_repository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidId_DeletesTaskAndSavesChanges()
    {
        var id = Guid.NewGuid();
        var task = new TodoTask { Id = id, Title = "Task to delete", CreatedAt = DateTime.UtcNow };
        _repository.GetTask(id).Returns(Task.FromResult(task));

        await _sut.ExecuteAsync(id);

        _repository.Received(1).DeleteTask(task);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task ExecuteAsync_DeletesTheCorrectTask()
    {
        var id = Guid.NewGuid();
        TodoTask? deleted = null;
        var task = new TodoTask { Id = id, Title = "Target", CreatedAt = DateTime.UtcNow };
        _repository.GetTask(id).Returns(Task.FromResult(task));
        _repository.When(r => r.DeleteTask(Arg.Any<TodoTask>())).Do(x => deleted = x.Arg<TodoTask>());

        await _sut.ExecuteAsync(id);

        Assert.Equal(id, deleted?.Id);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTaskNotFound_PropagatesException()
    {
        var id = Guid.NewGuid();
        _repository.GetTask(id).Throws(new KeyNotFoundException("Task not found"));

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.ExecuteAsync(id));
    }

    [Fact]
    public async Task ExecuteAsync_WhenTaskNotFound_DoesNotCallDeleteOrSave()
    {
        var id = Guid.NewGuid();
        _repository.GetTask(id).Throws(new KeyNotFoundException("Task not found"));

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.ExecuteAsync(id));

        _repository.DidNotReceive().DeleteTask(Arg.Any<TodoTask>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }
}
