using FluentAssertions;
using Moq;
using TaskFlow.Application.Common.Caching;
using TaskFlow.Application.Features.Tags.Commands.DeleteTag;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Tests.Features.Tags;

public sealed class DeleteTagHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ITagRepository> _mockTagRepo;
    private readonly Mock<ICacheService> _mockCache;
    private readonly DeleteTagCommandHandler _handler;

    public DeleteTagHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockTagRepo = new Mock<ITagRepository>();
        _mockCache = new Mock<ICacheService>();

        _mockUnitOfWork.Setup(u => u.Tags).Returns(_mockTagRepo.Object);

        _handler = new DeleteTagCommandHandler(_mockUnitOfWork.Object, _mockCache.Object);
    }

    [Fact]
    public async Task Handle_WhenTagExists_ReturnsSuccess()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var tag = new Tag { Name = "Urgente", Color = "#EF4444" };

        _mockTagRepo
            .Setup(r => r.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new DeleteTagCommand(tagId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenTagExists_DeletesAndInvalidatesCache()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var tag = new Tag { Name = "Urgente", Color = "#EF4444" };

        _mockTagRepo
            .Setup(r => r.GetByIdAsync(tagId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tag);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(new DeleteTagCommand(tagId), CancellationToken.None);

        // Assert — se debe llamar Delete() y luego invalidar el caché
        _mockTagRepo.Verify(r => r.Delete(tag), Times.Once);
        _mockCache.Verify(c => c.Remove("tags:all"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTagNotFound_ReturnsNotFoundError()
    {
        // Arrange — el tag no existe en la BD
        _mockTagRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        // Act
        var result = await _handler.Handle(new DeleteTagCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task Handle_WhenTagNotFound_DoesNotPersistAnything()
    {
        // Arrange
        _mockTagRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        // Act
        await _handler.Handle(new DeleteTagCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        _mockTagRepo.Verify(r => r.Delete(It.IsAny<Tag>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
