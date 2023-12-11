using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContexts.CommandHandlers;
using SubtitleFileCleanerWeb.Application.FileContexts.Commands;
using SubtitleFileCleanerWeb.Application.UnitTests.Fixtures;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.FileContexts.CommandHandlers;

public class TestUpdateFileContextNameHandler
{
    private readonly List<FileContext> _fileContexts;
    private readonly Mock<ApplicationDbContext> _dbContextMock;

    public TestUpdateFileContextNameHandler()
    {
        _fileContexts = FileContextFixture.GetListOfThree();

        _dbContextMock = new();
        _dbContextMock.Setup(db => db.FileContexts)
            .ReturnsDbSet(_fileContexts);
    }

    [Fact]
    public async Task Handle_WithFooName_ReturnValid()
    {
        // Arrange
        var contextToUpdate = _fileContexts.Last();

        var request = new UpdateFileContextName(contextToUpdate.FileContextId, "FooName");

        var handler = new UpdateFileContextNameHandler(_dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _dbContextMock.VerifyGet(
            db => db.FileContexts,
            Times.Exactly(2));

        _dbContextMock.Verify(
            db => db.FileContexts.Update(It.IsAny<FileContext>()),
            Times.Once());

        _dbContextMock.Verify(
            db => db.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            
            .Which.Should().Be(contextToUpdate);
    }

    [Fact]
    public async Task Handle_WithEmptyName_ReturnValidationError()
    {
        // Arrange
        var request = new UpdateFileContextName(_fileContexts.Last().FileContextId, string.Empty);

        var handler = new UpdateFileContextNameHandler(_dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _dbContextMock.VerifyGet(
            db => db.FileContexts,
            Times.Once());

        _dbContextMock.Verify(
            db => db.Update(It.IsAny<FileContext>()),
            Times.Never());

        _dbContextMock.Verify(
            db => db.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.ValidationError, "The provided name is either null or white space.")
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task Handle_WithNonExistentGuidId_ReturnNotFoundError()
    {
        // Arrange
        var fileContextId = Guid.Empty;

        var request = new UpdateFileContextName(Guid.Empty, "FooName");

        var handler = new UpdateFileContextNameHandler(_dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _dbContextMock.VerifyGet(
            db => db.FileContexts,
            Times.Once());

        _dbContextMock.Verify(
            db => db.FileContexts.Update(It.IsAny<FileContext>()),
            Times.Never());

        _dbContextMock.Verify(
            db => db.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.NotFound, $"No file context found with id: {fileContextId}")
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task Handle_WithUnexpectedError_ReturnUnknownError()
    {
        // Arrange
        var exceptionMessage = "Unexpected error occurred";
        _dbContextMock.SetupGet(db => db.FileContexts)
            .Throws(new Exception(exceptionMessage));

        var request = new UpdateFileContextName(Guid.Empty, "FooName");

        var handler = new UpdateFileContextNameHandler(_dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _dbContextMock.VerifyGet(
            db => db.FileContexts,
            Times.Once());

        _dbContextMock.Verify(
            db => db.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, exceptionMessage)
            .And.HaveDefaultPayload();
    }
}
