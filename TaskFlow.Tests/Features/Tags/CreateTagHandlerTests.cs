using FluentAssertions;
using Moq;
using TaskFlow.Application.Common.Caching;
using TaskFlow.Application.Features.Tags.Commands.CreateTag;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Tests.Features.Tags;

public sealed class CreateTagHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ITagRepository> _mockTagRepo;
    private readonly Mock<ICacheService> _mockCache;
    private readonly CreateTagCommandHandler _handler;

    public CreateTagHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockTagRepo = new Mock<ITagRepository>();
        _mockCache = new Mock<ICacheService>();

        _mockUnitOfWork.Setup(u => u.Tags).Returns(_mockTagRepo.Object);

        _handler = new CreateTagCommandHandler(_mockUnitOfWork.Object, _mockCache.Object);
    }

    [Fact]
    public async Task Handle_WhenNameIsAvailable_ReturnsSuccessWithNewId()
    {
        // Arrange — el nombre no existe aún en la BD
        _mockTagRepo
            .Setup(r => r.GetByNameAsync("Bug", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        _mockTagRepo
            .Setup(r => r.AddAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreateTagCommand("Bug", "#EF4444");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNameIsAvailable_InvalidatesTagsCache()
    {
        // Arrange
        _mockTagRepo
            .Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tag?)null);

        _mockTagRepo
            .Setup(r => r.AddAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(new CreateTagCommand("Feature", "#6366F1"), CancellationToken.None);

        // Assert — el caché de tags debe invalidarse al crear uno nuevo
        _mockCache.Verify(c => c.Remove("tags:all"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyTaken_ReturnsFailureWithConflictCode()
    {
        // Arrange — ya existe un tag con ese nombre
        var existingTag = new Tag { Name = "Bug", Color = "#EF4444" };

        _mockTagRepo
            .Setup(r => r.GetByNameAsync("Bug", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTag);

        var command = new CreateTagCommand("Bug", "#FF0000");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Tag.NameTaken");
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyTaken_DoesNotPersistAnything()
    {
        // Arrange
        _mockTagRepo
            .Setup(r => r.GetByNameAsync("Bug", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tag { Name = "Bug" });

        // Act
        await _handler.Handle(new CreateTagCommand("Bug", "#FF0000"), CancellationToken.None);

        // Assert — no se debe tocar la BD
        _mockTagRepo.Verify(r => r.AddAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
