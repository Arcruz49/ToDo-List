using NSubstitute;
using Xunit;
using TodoAPI.Application.DTOs;
using TodoAPI.Application.UseCases;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Enums;
using TodoAPI.Domain.Interfaces;
using TodoAPI.Domain.Interfaces.Persistence;

namespace TodoAPI.Tests.UseCases;

public class CreateTaskTests
{
    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateTask _sut;

    public CreateTaskTests()
    {
        _sut = new CreateTask(_unitOfWork, _repository);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_ReturnsMappedResponse()
    {
        var request = new TaskCreateRequest
        {
            Title = "Buy groceries",
            Description = "Milk and eggs",
            Color = "yellow"
        };

        var result = await _sut.ExecuteAsync(request);

        Assert.Equal("Buy groceries", result.Title);
        Assert.Equal("Milk and eggs", result.Description);
        Assert.Equal("yellow", result.Color);
    }

    [Fact]
    public async Task ExecuteAsync_NewTask_HasPendingStatusAndNoConcludedAt()
    {
        var request = new TaskCreateRequest { Title = "Task" };

        var result = await _sut.ExecuteAsync(request);

        Assert.Equal(TodoTaskStatus.Pending, result.Status);
        Assert.Null(result.ConcludedAt);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullOptionalFields_DefaultsToEmptyStrings()
    {
        var request = new TaskCreateRequest { Title = null, Description = null, Color = null };

        var result = await _sut.ExecuteAsync(request);

        Assert.Equal("", result.Title);
        Assert.Equal("", result.Description);
        Assert.Equal("", result.Color);
    }

    [Fact]
    public async Task ExecuteAsync_CallsRepositoryCreateAndSavesChanges()
    {
        var request = new TaskCreateRequest { Title = "Task" };

        await _sut.ExecuteAsync(request);

        _repository.Received(1).CreateTask(Arg.Any<TodoTask>());
        await _unitOfWork.Received(1).SaveChangesAsync();
    }
}
