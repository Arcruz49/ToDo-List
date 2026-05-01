using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;
using NSubstitute.ExceptionExtensions;
using TodoAPI.Application.DTOs;
using TodoAPI.Application.Interfaces;
using TodoAPI.Controllers;
using TodoAPI.Domain.Enums;

namespace TodoAPI.Tests.Controllers;

public class TaskControllerTests
{
    private readonly ICreateTask _createTask = Substitute.For<ICreateTask>();
    private readonly IUpdateTask _updateTask = Substitute.For<IUpdateTask>();
    private readonly IGetTask _getTask = Substitute.For<IGetTask>();
    private readonly IGetTasks _getTasks = Substitute.For<IGetTasks>();
    private readonly IDeleteTask _deleteTask = Substitute.For<IDeleteTask>();
    private readonly TaskController _sut;

    public TaskControllerTests()
    {
        _sut = new TaskController(_createTask, _updateTask, _getTask, _getTasks, _deleteTask);
    }

    private static TaskResponse BuildResponse(Guid? id = null, string title = "Task") =>
        new(id ?? Guid.NewGuid(), title, "Description", DateTime.UtcNow, null, TodoTaskStatus.Pending, "yellow", null);

    // --- CreateTask ---

    [Fact]
    public async Task CreateTask_WithValidRequest_Returns201Created()
    {
        var response = BuildResponse(title: "New task");
        _createTask.ExecuteAsync(Arg.Any<TaskCreateRequest>()).Returns(response);

        var result = await _sut.CreateTask(new TaskCreateRequest { Title = "New task" });

        var created = Assert.IsType<CreatedResult>(result);
        Assert.Equal(201, created.StatusCode);
        Assert.Equal(response, created.Value);
    }

    [Fact]
    public async Task CreateTask_WithEmptyTitle_Returns400BadRequest()
    {
        var result = await _sut.CreateTask(new TaskCreateRequest { Title = "" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateTask_WithWhitespaceTitle_Returns400BadRequest()
    {
        var result = await _sut.CreateTask(new TaskCreateRequest { Title = "   " });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateTask_WithNullTitle_Returns400BadRequest()
    {
        var result = await _sut.CreateTask(new TaskCreateRequest { Title = null });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateTask_LocationHeaderPointsToNewResource()
    {
        var id = Guid.NewGuid();
        _createTask.ExecuteAsync(Arg.Any<TaskCreateRequest>()).Returns(BuildResponse(id));

        var result = await _sut.CreateTask(new TaskCreateRequest { Title = "Task" });

        var created = Assert.IsType<CreatedResult>(result);
        Assert.Equal($"/tasks/{id}", created.Location);
    }

    // --- UpdateTask ---

    [Fact]
    public async Task UpdateTask_WithValidRequest_Returns200Ok()
    {
        var response = BuildResponse(title: "Updated");
        _updateTask.ExecuteAsync(Arg.Any<TaskEditRequest>()).Returns(response);

        var result = await _sut.UpdateTask(new TaskEditRequest { Title = "Updated", Status = TodoTaskStatus.InProgress });

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, ok.Value);
    }

    [Fact]
    public async Task UpdateTask_WithEmptyTitle_Returns400BadRequest()
    {
        var result = await _sut.UpdateTask(new TaskEditRequest { Title = "" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateTask_WithWhitespaceTitle_Returns400BadRequest()
    {
        var result = await _sut.UpdateTask(new TaskEditRequest { Title = "  " });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // --- GetTask ---

    [Fact]
    public async Task GetTask_WithExistingId_Returns200Ok()
    {
        var id = Guid.NewGuid();
        var response = BuildResponse(id);
        _getTask.ExecuteAsync(id).Returns(response);

        var result = await _sut.GetTask(id);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, ok.Value);
    }

    [Fact]
    public async Task GetTask_WhenUseCaseThrowsKeyNotFound_PropagatesException()
    {
        var id = Guid.NewGuid();
        _getTask.ExecuteAsync(id).Throws(new KeyNotFoundException("Task not found"));

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.GetTask(id));
    }

    // --- GetTasks ---

    [Fact]
    public async Task GetTasks_Returns200OkWithList()
    {
        var responses = new List<TaskResponse> { BuildResponse(), BuildResponse() };
        _getTasks.ExecuteAsync().Returns(responses);

        var result = await _sut.GetTasks();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(responses, ok.Value);
    }

    // --- DeleteTask ---

    [Fact]
    public async Task DeleteTask_WithValidId_Returns204NoContent()
    {
        var id = Guid.NewGuid();

        var result = await _sut.DeleteTask(id);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteTask_DelegatesIdToUseCase()
    {
        var id = Guid.NewGuid();

        await _sut.DeleteTask(id);

        await _deleteTask.Received(1).ExecuteAsync(id);
    }

    [Fact]
    public async Task DeleteTask_WhenUseCaseThrowsKeyNotFound_PropagatesException()
    {
        var id = Guid.NewGuid();
        _deleteTask.ExecuteAsync(id).Throws(new KeyNotFoundException("Task not found"));

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteTask(id));
    }
    
}
