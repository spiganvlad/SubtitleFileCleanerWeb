using FluentAssertions;
using FluentAssertions.Extensions;
using MediatR;
using Moq;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.Commands;
using SubtitleFileCleanerWeb.Application.FileContexts.CommandHandlers;
using SubtitleFileCleanerWeb.Application.FileContexts.Commands;
using SubtitleFileCleanerWeb.Application.Models;
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
        var content = new byte[] { 1, 2, 3, 4, 5 };
        var contentStream = new MemoryStream(content);
        var cancellationToken = new CancellationToken();
        
        var mediatorResult = new OperationResult<FileContent>
        { Payload = FileContent.Create(new MemoryStream(content, false)) };
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateFileContent>(), cancellationToken))
            .ReturnsAsync(mediatorResult);

        var request = new CreateFileContext(contextName, contentStream);

        var handler = new CreateFileContextHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFileContent>(), It.IsAny<CancellationToken>()), Times.Once());

        _dbContextMock.Verify(db => db.FileContexts, Times.Once());
        _dbContextMock.Verify(db => db.FileContexts.Add(It.IsAny<FileContext>()), Times.Once);
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());

        var payload = result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            .Which;

        payload.FileContextId.Should().NotBeEmpty();
        payload.FileContent.Should().NotBeNull().And.Be(mediatorResult.Payload);
        payload.Name.Should().NotBeNull().And.Be(request.FileName + ".txt");
        payload.ContentSize.Should().Be(content.Length);
        payload.DateCreated.Should().BeCloseTo(DateTime.UtcNow, 1.Minutes());
        payload.DateModified.Should().BeCloseTo(DateTime.UtcNow, 1.Minutes());

        _fileContexts.Should().ContainSingle();
        _fileContexts[0].Should().NotBeNull().And.Be(result.Payload);
    }

    [Fact]
    public async Task Handle_WithNullName_ReturnValidationError()
    {
        // Arrange
        var request = new CreateFileContext(null!, new MemoryStream(new byte[] { 1 }));
        var cancellationToken = new CancellationToken();

        var handler = new CreateFileContextHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFileContent>(), It.IsAny<CancellationToken>()), Times.Never());

        _dbContextMock.Verify(db => db.FileContexts, Times.Never());
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.ValidationError, "File context name cannot be null.")
            .And.HaveDefaultPayload();

        _fileContexts.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithEmptyName_ReturnValidationError()
    {
        // Arrange
        var request = new CreateFileContext(string.Empty, new MemoryStream(new byte[] { 1 }));
        var cancellationToken = new CancellationToken();

        var handler = new CreateFileContextHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFileContent>(), It.IsAny<CancellationToken>()), Times.Never());

        _dbContextMock.Verify(db => db.FileContexts, Times.Never());
        _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.ValidationError, "File context name cannot be empty.")
            .And.HaveDefaultPayload();

        _fileContexts.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithCreateContentError_RaiseContentError()
    {
        // Arrange
        var request = new CreateFileContext("FooName", new MemoryStream(new byte[] { 1 }));
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

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, errorMessage)
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task Handle_WithUnexpectedError_ReturnUnknownError()
    {
        // Arrange
        var request = new CreateFileContext("FooName", new MemoryStream(new byte[] { 1 }));
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

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, exceptionMessage)
            .And.HaveDefaultPayload();
    }
}
