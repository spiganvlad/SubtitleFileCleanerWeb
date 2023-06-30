using FluentAssertions;
using FluentAssertions.Extensions;
using MediatR;
using Moq;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.Commands;
using SubtitleFileCleanerWeb.Application.FileContexts.CommandHandlers;
using SubtitleFileCleanerWeb.Application.FileContexts.Commands;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.Extensions;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.FileContexts.CommandHandlers;

public class TestCreateFileContextHandler
{
    private readonly List<FileContext> _fileContexts;
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly Mock<IMediator> _mediatorMock;

    public TestCreateFileContextHandler()
    {
        _fileContexts = new List<FileContext>();

        _dbContextMock = new Mock<ApplicationDbContext>();
        _dbContextMock.Setup(db => db.FileContexts.Add(It.IsAny<FileContext>()))
            .Callback<FileContext>(_fileContexts.Add);

        _mediatorMock = new Mock<IMediator>();
    }

    [Fact]
    public async Task Handle_WithValidParameters_ReturnValid()
    {
        // Arrange
        var contextName = "FooName";
        var contextContent = new MemoryStream();

        var request = new CreateFileContext(contextName, contextContent);
        var cancellationToken = new CancellationToken();

        var mediatorResult = new OperationResult<FileContent>
        { Payload = FileContent.Create(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }, false)) };
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateFileContent>(), cancellationToken))
            .ReturnsAsync(mediatorResult);

        var handler = new CreateFileContextHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFileContent>(), It.IsAny<CancellationToken>()), Times.Once());

        _dbContextMock.Verify(db => db.FileContexts, Times.Once());
        _dbContextMock.Verify(db => db.FileContexts.Add(It.IsAny<FileContext>()), Times.Once);
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull().And.ContainsNoErrors();
        result.Payload.Should().NotBeNull();
        result.Payload!.FileContextId.Should().NotBeEmpty();
        result.Payload.Content.Should().NotBeNull().And.Be(mediatorResult.Payload);
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
        var request = new CreateFileContext(null!, new MemoryStream());
        var cancellationToken = new CancellationToken();

        var handler = new CreateFileContextHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFileContent>(), It.IsAny<CancellationToken>()), Times.Never());

        _dbContextMock.Verify(db => db.FileContexts, Times.Never());
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());

        result.Should().ContainSingleError(ErrorCode.ValidationError, "File context name cannot be null.");
        result.Payload.Should().BeNull();

        _fileContexts.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithEmptyName_ReturnValidationError()
    {
        // Arrange
        var request = new CreateFileContext(string.Empty, new MemoryStream());
        var cancellationToken = new CancellationToken();

        var handler = new CreateFileContextHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFileContent>(), It.IsAny<CancellationToken>()), Times.Never());

        _dbContextMock.Verify(db => db.FileContexts, Times.Never());
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());

        result.Should().ContainSingleError(ErrorCode.ValidationError, "File context name cannot be empty.");
        result.Payload.Should().BeNull();

        _fileContexts.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithCreateContentError_RaiseContentError()
    {
        // Arrange
        var request = new CreateFileContext("FooName", new MemoryStream());
        var cancellationToken = new CancellationToken();

        var errorMessage = "Test unexpected error occurred";
        var mediatorResult = new OperationResult<FileContent>();
        mediatorResult.AddError(ErrorCode.UnknownError, errorMessage);
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateFileContent>(), cancellationToken))
            .ReturnsAsync(mediatorResult);

        var handler = new CreateFileContextHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFileContent>(), It.IsAny<CancellationToken>()), Times.Once);

        _dbContextMock.Verify(db => db.FileContexts, Times.Never());
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());

        result.Should().NotBeNull().And.ContainSingleError(ErrorCode.UnknownError, errorMessage);
        result.Payload.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithUnexpectedError_ReturnUnknownError()
    {
        // Arrange
        var request = new CreateFileContext("FooName", new MemoryStream());
        var cancellationToken = new CancellationToken();

        var exceptionMessage = "Unexpected error occurred";
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateFileContent>(), cancellationToken))
            .ThrowsAsync(new Exception(exceptionMessage));

        var handler = new CreateFileContextHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFileContent>(), It.IsAny<CancellationToken>()), Times.Once());

        _dbContextMock.Verify(db => db.FileContexts, Times.Never());
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());

        result.Should().NotBeNull().And.ContainSingleError(ErrorCode.UnknownError, exceptionMessage);
        result.Payload.Should().BeNull();
    }
}
