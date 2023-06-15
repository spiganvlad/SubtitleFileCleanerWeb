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

public class TestUpdateFileContextHandler
{
    private readonly List<FileContext> _fileContexts;
    private readonly Mock<ApplicationDbContext> _dbContextMock;

    public TestUpdateFileContextHandler()
    {
        _fileContexts = FileContextFixture.GetListOfThree();

        _dbContextMock = new Mock<ApplicationDbContext>();
        _dbContextMock.Setup(db => db.FileContexts)
            .ReturnsDbSet(_fileContexts);
    }

    [Fact]
    public async Task Handle_WithFooName_ReturnValid()
    {
        // Arrange
        var contextToUpdate = _fileContexts.Last();
        var request = new UpdateFileContextName(contextToUpdate.FileContextId, "FooName");
        var cancellationToken = new CancellationToken();

        var handler = new UpdateFileContextNameHandler(_dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _dbContextMock.Verify(db => db.FileContexts, Times.Exactly(2));
        _dbContextMock.Verify(db => db.FileContexts.Update(It.IsAny<FileContext>()), Times.Once());
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        result.Should().NotBeNull().And.ContainsNoErrors();
        result.Payload.Should().NotBeNull().And.Be(contextToUpdate);
    }

    [Fact]
    public async Task Handle_WithEmptyName_ReturnValidationError()
    {
        // Arrange
        var request = new UpdateFileContextName(_fileContexts.Last().FileContextId, string.Empty);
        var cancellationToken = new CancellationToken();

        var handler = new UpdateFileContextNameHandler(_dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _dbContextMock.Verify(db => db.FileContexts, Times.Once());
        _dbContextMock.Verify(db => db.Update(It.IsAny<FileContext>()), Times.Never());
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());

        result.Should().ContainSingleError(ErrorCode.ValidationError, "The provided name is either null or white space");
        result.Payload.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentGuidId_ReturnNotFoundError()
    {
        // Arrange
        var fileContextId = Guid.Empty;
        var request = new UpdateFileContextName(fileContextId, "FooName");
        var cancellationToken = new CancellationToken();

        var handler = new UpdateFileContextNameHandler(_dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _dbContextMock.Verify(db => db.FileContexts, Times.Once);
        _dbContextMock.Verify(db => db.FileContexts.Update(It.IsAny<FileContext>()), Times.Never());
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());

        result.Should().ContainSingleError(ErrorCode.NotFound, $"No file context found with id: {fileContextId}");
        result.Payload.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithUnknownError_RerurnUnknownError()
    {
        // Arrange
        var request = new UpdateFileContextName(Guid.Empty, "FooName");
        var cancellationToken = new CancellationToken();

        var exceptionMessage = "Unexpected error occurred";
        _dbContextMock.Setup(db => db.FileContexts)
            .Throws(new Exception(exceptionMessage));
        var handler = new UpdateFileContextNameHandler(_dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _dbContextMock.Verify(db => db.FileContexts, Times.Once());
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());

        result.Should().NotBeNull().And.ContainSingleError(ErrorCode.UnknownError, exceptionMessage);
        result.Payload.Should().BeNull();
    }
}
