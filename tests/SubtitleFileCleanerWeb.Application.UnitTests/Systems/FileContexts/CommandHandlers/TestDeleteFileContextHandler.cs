using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContexts.CommandHandlers;
using SubtitleFileCleanerWeb.Application.FileContexts.Commands;
using SubtitleFileCleanerWeb.Application.UnitTests.Fixtures;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.FluentOperationResult;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.FileContexts.CommandHandlers;

public class TestDeleteFileContextHandler
{
    private readonly List<FileContext> _fileContexts;
    private readonly Mock<ApplicationDbContext> _dbContextMock;

    public TestDeleteFileContextHandler()
    {
        _fileContexts = FileContextFixture.GetListOfThree();

        _dbContextMock = new Mock<ApplicationDbContext>();
        _dbContextMock.Setup(dbContext => dbContext.FileContexts).
            ReturnsDbSet(_fileContexts);
        _dbContextMock.Setup(db => db.FileContexts.Remove(It.IsAny<FileContext>()))
            .Callback<FileContext>(fc => _fileContexts.Remove(fc));
    }

    [Fact]
    public async Task Handle_WithValidGuidId_ReturnValid()
    {
        // Arrange
        var handler = new DeleteFileContextHandler(_dbContextMock.Object);

        var contextToRemove = _fileContexts.Last();
        var request = new DeleteFileContext(contextToRemove.FileContextId);
        var cancellationToken = new CancellationToken();

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _dbContextMock.Verify(db => db.FileContexts, Times.Exactly(2));
        _dbContextMock.Verify(db => db.FileContexts.Remove(It.IsAny<FileContext>()), Times.Once());
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull().And.ContainsNoErrors();
        result.Payload.Should().NotBeNull().And.Be(contextToRemove);

        _fileContexts.Should().HaveCount(2).And.NotContain(contextToRemove);
    }

    [Fact]
    public async Task Handle_WithNonExistentGuidId_ReturnNotFoundError()
    {
        // Arrange
        var handler = new DeleteFileContextHandler(_dbContextMock.Object);

        var request = new DeleteFileContext(Guid.Empty);
        var cancellationToken = new CancellationToken();

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _dbContextMock.Verify(db => db.FileContexts, Times.Once());
        _dbContextMock.Verify(db => db.FileContexts.Remove(It.IsAny<FileContext>()), Times.Never());
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());

        result.Should().ContainSingleError(ErrorCode.NotFound, $"No file context found with id: {request.FileContextId}");

        _fileContexts.Should().HaveCount(3);
    }
}
