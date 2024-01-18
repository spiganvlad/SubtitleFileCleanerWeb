using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.Queries;
using SubtitleFileCleanerWeb.Application.FileContexts.Queries;
using SubtitleFileCleanerWeb.Application.FileContexts.QueryHandlers;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.UnitTests.Fixtures;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.FileContexts.QueryHandlers;

public class TestGetFileContextWithContentByIdHandler
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ApplicationDbContext> _dbContextMock;

    public TestGetFileContextWithContentByIdHandler()
    {
        _mediatorMock = new();
        _dbContextMock = new();
    }

    [Fact]
    public async Task Handle_WithValidParameters_ReturnValid()
    {
        // Arrange
        var fileContexts = FileContextFixture.GetListOfThree();
        var searchedContext = fileContexts.Last();
        var path = "Unauthorized\\" + searchedContext.FileContextId.ToString();

        var searchedContent = FileContent.Create(new MemoryStream(new byte[] { 1 }, false));

        _dbContextMock.SetupGet(db => db.FileContexts)
            .ReturnsDbSet(fileContexts);

        _mediatorMock.Setup(
            m => m.Send(
                It.Is<GetFileContentById>(x => x.Path == path),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OperationResult<FileContent>() { Payload = searchedContent });

        var request = new GetFileContextWithContentById(searchedContext.FileContextId);

        var handler = new GetFileContextWithContentByIdHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _dbContextMock.VerifyGet(
            db => db.FileContexts,
            Times.Once());

        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<GetFileContentById>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        var payload = result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            .Which;

        payload.Should().Be(searchedContext);
        payload.FileContent.Should().NotBeNull()
            .And.Be(searchedContent);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnNotFoundError()
    {
        // Arrange
        _dbContextMock.SetupGet(db => db.FileContexts)
            .ReturnsDbSet(Enumerable.Empty<FileContext>());

        var request = new GetFileContextWithContentById(Guid.Empty);

        var handler = new GetFileContextWithContentByIdHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _dbContextMock.VerifyGet(
            db => db.FileContexts,
            Times.Once());

        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<GetFileContentById>(),
                It.IsAny<CancellationToken>()),
            Times.Never());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.NotFound, $"No file context found with id: {Guid.Empty}.")
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task Handle_WithFileContentError_RaiseError()
    {
        // Arrange
        var fileContexts = FileContextFixture.GetListOfThree();
        var searchedFileContext = fileContexts.Last();
        var path = "Unauthorized\\" + searchedFileContext.FileContextId.ToString();

        _dbContextMock.SetupGet(db => db.FileContexts)
            .ReturnsDbSet(fileContexts);

        var fileContentResult = new OperationResult<FileContent>();

        var errorCode = (ErrorCode)(-1);
        var errorMessage = "Test unexpected error occurred.";
        fileContentResult.AddError(errorCode, errorMessage);

        _mediatorMock.Setup(
            m => m.Send(
                It.Is<GetFileContentById>(x => x.Path == path),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileContentResult);

        var request = new GetFileContextWithContentById(searchedFileContext.FileContextId);

        var handler = new GetFileContextWithContentByIdHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _dbContextMock.VerifyGet(
            db => db.FileContexts,
            Times.Once());
        
        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<GetFileContentById>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(errorCode, errorMessage)
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task Handle_WithUnexpectedException_ReturnUnknownError()
    {
        // Arrange
        var exceptionMessage = "Test unexpected error occurred.";
        _dbContextMock.SetupGet(db => db.FileContexts)
            .Throws(new Exception(exceptionMessage));

        var request = new GetFileContextWithContentById(Guid.Empty);

        var handler = new GetFileContextWithContentByIdHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _dbContextMock.VerifyGet(
            db => db.FileContexts,
            Times.Once());

        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<GetFileContentById>(),
                It.IsAny<CancellationToken>()),
            Times.Never());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, exceptionMessage)
            .And.HaveDefaultPayload();
    }
}
