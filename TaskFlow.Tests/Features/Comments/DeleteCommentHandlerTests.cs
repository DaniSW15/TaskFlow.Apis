using FluentAssertions;
using Moq;
using TaskFlow.Application.Features.Comments.Commands.DeleteComment;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Tests.Features.Comments;

public sealed class DeleteCommentHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ICommentRepository> _mockCommentRepo;
    private readonly DeleteCommentCommandHandler _handler;

    public DeleteCommentHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCommentRepo = new Mock<ICommentRepository>();

        _mockUnitOfWork.Setup(u => u.Comments).Returns(_mockCommentRepo.Object);

        _handler = new DeleteCommentCommandHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenAuthorDeletesOwnComment_ReturnsSuccess()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var comment = new Comment { Content = "Texto", AuthorId = authorId };

        _mockCommentRepo
            .Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new DeleteCommentCommand(commentId, authorId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockCommentRepo.Verify(r => r.Delete(comment), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDifferentUserTriesToDelete_ReturnsForbidden()
    {
        // Arrange — el comentario pertenece a otro usuario
        var realAuthorId = Guid.NewGuid();
        var intruderUserId = Guid.NewGuid();
        var commentId = Guid.NewGuid();

        var comment = new Comment { Content = "Texto", AuthorId = realAuthorId };

        _mockCommentRepo
            .Setup(r => r.GetByIdAsync(commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        // Act
        var result = await _handler.Handle(new DeleteCommentCommand(commentId, intruderUserId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Forbidden");
    }

    [Fact]
    public async Task Handle_WhenDifferentUserTriesToDelete_DoesNotPersistAnything()
    {
        // Arrange
        var comment = new Comment { Content = "Texto", AuthorId = Guid.NewGuid() };

        _mockCommentRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(comment);

        // Act
        await _handler.Handle(new DeleteCommentCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        // Assert — la BD no debe modificarse
        _mockCommentRepo.Verify(r => r.Delete(It.IsAny<Comment>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCommentNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _mockCommentRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Comment?)null);

        // Act
        var result = await _handler.Handle(
            new DeleteCommentCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }
}
