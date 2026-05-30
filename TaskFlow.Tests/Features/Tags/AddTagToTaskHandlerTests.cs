using FluentAssertions;
using Moq;
using TaskFlow.Application.Features.Tags.Commands.AddTagToTask;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Tests.Features.Tags;

/// <summary>
/// Tests para la relación N:M entre TaskItem y Tag (tabla TaskItemTags).
/// Este handler es interesante porque no toca un repositorio de join directamente
/// — EF Core detecta el cambio en la colección y escribe en TaskItemTags.
/// </summary>
public sealed class AddTagToTaskHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ITaskRepository> _mockTaskRepo;
    private readonly Mock<ITagRepository> _mockTagRepo;
    private readonly AddTagToTaskCommandHandler _handler;

    public AddTagToTaskHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockTaskRepo = new Mock<ITaskRepository>();
        _mockTagRepo = new Mock<ITagRepository>();

        _mockUnitOfWork.Setup(u => u.Tasks).Returns(_mockTaskRepo.Object);
        _mockUnitOfWork.Setup(u => u.Tags).Returns(_mockTagRepo.Object);

        _handler = new AddTagToTaskCommandHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenTaskAndTagExist_AddsTagToCollectionAndReturnsSuccess()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        var task = new TaskItem { Title = "Tarea", Tags = [] };
        var tag = new Tag { Name = "Bug", Color = "#EF4444" };

        _mockTaskRepo
            .Setup(r => r.GetByIdWithTagsAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        _mockTagRepo
            .Setup(r => r.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new AddTagToTaskCommand(taskId, tagId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Tags.Should().Contain(tag); // El tag fue añadido a la colección
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTagAlreadyOnTask_ReturnsSuccessWithoutDuplicating()
    {
        // Arrange — el tag ya está en la tarea (comportamiento idempotente)
        var taskId = Guid.NewGuid();
        var tag = new Tag { Name = "Bug", Color = "#EF4444" };

        // Usamos tag.Id (auto-generado) para que el handler lo reconozca como duplicado
        var tagId = tag.Id;

        // La tarea ya tiene ese tag
        var task = new TaskItem { Title = "Tarea", Tags = [tag] };

        _mockTaskRepo
            .Setup(r => r.GetByIdWithTagsAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        _mockTagRepo
            .Setup(r => r.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        // Act
        var result = await _handler.Handle(new AddTagToTaskCommand(taskId, tagId), CancellationToken.None);

        // Assert — éxito sin duplicar ni persistir nada
        result.IsSuccess.Should().BeTrue();
        task.Tags.Should().HaveCount(1); // No se duplicó
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTaskNotFound_ReturnsFailure()
    {
        // Arrange
        _mockTaskRepo
            .Setup(r => r.GetByIdWithTagsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        // Act
        var result = await _handler.Handle(
            new AddTagToTaskCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Tag.TaskNotFound");
    }

    [Fact]
    public async Task Handle_WhenTagNotFound_ReturnsFailure()
    {
        // Arrange — la tarea existe pero el tag no
        _mockTaskRepo
            .Setup(r => r.GetByIdWithTagsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskItem { Title = "Tarea", Tags = [] });

        _mockTagRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        // Act
        var result = await _handler.Handle(
            new AddTagToTaskCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Tag.NotFound");
    }
}
