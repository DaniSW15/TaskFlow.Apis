using FluentAssertions;
using Moq;
using TaskFlow.Application.Features.Comments.Commands.AddComment;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Tests.Features.Comments;

public sealed class AddCommentHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ITaskRepository> _mockTaskRepo;
    private readonly Mock<ICommentRepository> _mockCommentRepo;
    private readonly AddCommentCommandHandler _handler;

    public AddCommentHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockTaskRepo = new Mock<ITaskRepository>();
        _mockCommentRepo = new Mock<ICommentRepository>();

        _mockUnitOfWork.Setup(u => u.Tasks).Returns(_mockTaskRepo.Object);
        _mockUnitOfWork.Setup(u => u.Comments).Returns(_mockCommentRepo.Object);

        _handler = new AddCommentCommandHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenTaskExists_CreatesCommentAndReturnsNewId()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var task = new TaskItem { Title = "Implementar login" };

        _mockTaskRepo
            .Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        _mockCommentRepo
            .Setup(r => r.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new AddCommentCommand(taskId, "Revisar validación del token", authorId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenTaskExists_PersistsCommentWithCorrectData()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var capturedComment = default(Comment);

        _mockTaskRepo
            .Setup(r => r.GetByIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskItem { Title = "Tarea" });

        _mockCommentRepo
            .Setup(r => r.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()))
            .Callback<Comment, CancellationToken>((c, _) => capturedComment = c)
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new AddCommentCommand(taskId, "Mi comentario", authorId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — verificar que el comentario persistido tiene los datos correctos
        capturedComment.Should().NotBeNull();
        capturedComment!.Content.Should().Be("Mi comentario");
        capturedComment.TaskItemId.Should().Be(taskId);
        capturedComment.AuthorId.Should().Be(authorId);
    }

    [Fact]
    public async Task Handle_WhenTaskNotFound_ReturnsFailure()
    {
        // Arrange — la tarea no existe
        _mockTaskRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        var command = new AddCommentCommand(Guid.NewGuid(), "Comentario", Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Comment.TaskNotFound");
    }

    [Fact]
    public async Task Handle_WhenTaskNotFound_DoesNotPersistAnything()
    {
        // Arrange
        _mockTaskRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskItem?)null);

        // Act
        await _handler.Handle(new AddCommentCommand(Guid.NewGuid(), "x", Guid.NewGuid()), CancellationToken.None);

        // Assert
        _mockCommentRepo.Verify(r => r.AddAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
