using FluentAssertions;
using FluentAssertions.Extensions;
using Moq;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContexts.CommandHandlers;
using SubtitleFileCleanerWeb.Application.FileContexts.Commands;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.FluentOperationResult;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.FileContexts.CommandHandlers;

public class TestCreateFileContextHandler
{
    private readonly List<FileContext> _fileContexts;
    private readonly Mock<ApplicationDbContext> _dbContextMock;

    public TestCreateFileContextHandler()
    {
        _fileContexts = new List<FileContext>();

        _dbContextMock = new Mock<ApplicationDbContext>();
        _dbContextMock.Setup(db => db.FileContexts.Add(It.IsAny<FileContext>()))
            .Callback<FileContext>(_fileContexts.Add);
    }

    [Fact]
    public async Task Handle_WithFooName_ReturnValid()
    {
        // Arrange
        var request = new CreateFileContext("FooName");
        var cancellationToken = new CancellationToken();

        var handler = new CreateFileContextHandler(_dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _dbContextMock.Verify(db => db.FileContexts, Times.Once());
        _dbContextMock.Verify(db => db.FileContexts.Add(It.IsAny<FileContext>()), Times.Once);
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull().And.ContainsNoErrors();
        result.Payload.Should().NotBeNull();
        result.Payload!.FileContextId.Should().NotBeEmpty();
        result.Payload.Name.Should().Be(request.FileName);
        result.Payload.DateCreated.Should().BeCloseTo(DateTime.UtcNow, 1.Minutes());
        result.Payload.DateModified.Should().BeCloseTo(DateTime.UtcNow, 1.Minutes());

        _fileContexts.Should().ContainSingle();
        _fileContexts[0].Should().NotBeNull().And.Be(result.Payload);
    }

    [Fact]
    public async Task Handle_WithNullName_ReturnValidationError()
    {
        // Arrange
        var request = new CreateFileContext(null!);
        var cancellationToken = new CancellationToken();

        var handler = new CreateFileContextHandler(_dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _dbContextMock.Verify(db => db.FileContexts, Times.Never());
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());

        result.Should().ContainSingleError(ErrorCode.ValidationError, "File context name cannot be null");
        result.Payload.Should().BeNull();

        _fileContexts.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithEmptyName_ReturnValidationError()
    {
        // Arrange
        var request = new CreateFileContext(string.Empty);
        var cancellationToken = new CancellationToken();

        var handler = new CreateFileContextHandler(_dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _dbContextMock.Verify(db => db.FileContexts, Times.Never());
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());

        result.Should().ContainSingleError(ErrorCode.ValidationError, "File context name cannot be empty");
        result.Payload.Should().BeNull();

        _fileContexts.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithUnknownError_ReturnUnknownError()
    {
        // Arrange
        var request = new CreateFileContext("FooName");
        var cancellationToken = new CancellationToken();

        var exceptionMessage = "Unexpected error occurred";
        _dbContextMock.Setup(db => db.FileContexts).Throws(new Exception(exceptionMessage));
        var handler = new CreateFileContextHandler(_dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _dbContextMock.Verify(db => db.FileContexts, Times.Once());
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());

        result.Should().NotBeNull().And.ContainSingleError(ErrorCode.UnknownError, exceptionMessage);
        result.Payload.Should().BeNull();
    }
}
