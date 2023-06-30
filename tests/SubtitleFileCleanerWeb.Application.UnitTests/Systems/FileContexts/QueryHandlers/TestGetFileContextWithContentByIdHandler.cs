using FluentAssertions;
using MediatR;
using Moq;
using Moq.EntityFrameworkCore;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.Queries;
using SubtitleFileCleanerWeb.Application.FileContexts.Queries;
using SubtitleFileCleanerWeb.Application.FileContexts.QueryHandlers;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.UnitTests.Fixtures;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.Extensions;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.FileContexts.QueryHandlers;

public class TestGetFileContextWithContentByIdHandler
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ApplicationDbContext> _dbContextMock;

    public TestGetFileContextWithContentByIdHandler()
    {
        _mediatorMock = new Mock<IMediator>();
        _dbContextMock = new Mock<ApplicationDbContext>();
    }

    [Fact]
    public async Task Handle_WithValidParameters_ReturnValid()
    {
        // Arrange
        var fileContexts = FileContextFixture.GetListOfThree();
        var searchedContext = fileContexts.Last();
        var searchedContent = FileContent.Create(new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }, false));
        var cancellationToken = new CancellationToken();

        _dbContextMock.Setup(db => db.FileContexts)
            .ReturnsDbSet(fileContexts);

        var getFileContent = new GetFileContentById(searchedContext.FileContextId.ToString());
        var fileContentResult = new OperationResult<FileContent>()
        { Payload = searchedContent };
        _mediatorMock.Setup(m => m.Send(getFileContent, cancellationToken))
            .ReturnsAsync(fileContentResult);

        var request = new GetFileContextWithContentById(searchedContext.FileContextId);

        var handler = new GetFileContextWithContentByIdHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _dbContextMock.Verify(db => db.FileContexts, Times.Once());

        _mediatorMock.Verify(m => m.Send(It.IsAny<GetFileContentById>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull().And.ContainsNoErrors();
        result.Payload.Should().NotBeNull().And.Be(searchedContext);
        result.Payload!.Content.Should().Be(searchedContent);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnNotFoundError()
    {
        // Arrange
        var cancellationToken = new CancellationToken();

        _dbContextMock.Setup(db => db.FileContexts)
            .ReturnsDbSet(Enumerable.Empty<FileContext>());

        var request = new GetFileContextWithContentById(Guid.Empty);

        var handler = new GetFileContextWithContentByIdHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _dbContextMock.Verify(db => db.FileContexts, Times.Once());

        _mediatorMock.Verify(m => m.Send(It.IsAny<GetFileContentById>(), It.IsAny<CancellationToken>()), Times.Never());

        result.Should().NotBeNull().And.ContainSingleError(ErrorCode.NotFound, $"No file context found with id: {Guid.Empty}");
        result.Payload.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithFileContentError_RaiseError()
    {
        // Arrange
        var fileContexts = FileContextFixture.GetListOfThree();
        var searchedFileContext = fileContexts.Last();
        var cancellationToken = new CancellationToken();

        _dbContextMock.Setup(db => db.FileContexts)
            .ReturnsDbSet(fileContexts);

        var getFileContent = new GetFileContentById(searchedFileContext.FileContextId.ToString());
        var errorMessage = "Test unexpected error occurred";
        var fileContentResult = new OperationResult<FileContent>();
        fileContentResult.AddError(ErrorCode.UnknownError, errorMessage);
        _mediatorMock.Setup(m => m.Send(getFileContent, cancellationToken))
            .ReturnsAsync(fileContentResult);

        var request = new GetFileContextWithContentById(searchedFileContext.FileContextId);

        var handler = new GetFileContextWithContentByIdHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _dbContextMock.Verify(db => db.FileContexts, Times.Once());
        
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetFileContentById>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull().And.ContainSingleError(ErrorCode.UnknownError, errorMessage);
        result.Payload.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithUnexpectedException_ReturnUnknownError()
    {
        // Arrange
        var cancellationToken = new CancellationToken();

        var exceptionMessage = "Test unexpected error occurred";
        _dbContextMock.Setup(db => db.FileContexts)
            .Throws(new Exception(exceptionMessage));

        var request = new GetFileContextWithContentById(Guid.Empty);

        var handler = new GetFileContextWithContentByIdHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _dbContextMock.Verify(db => db.FileContexts, Times.Once());

        _mediatorMock.Verify(m => m.Send(It.IsAny<GetFileContentById>(), It.IsAny<CancellationToken>()), Times.Never());

        result.Should().NotBeNull().And.ContainSingleError(ErrorCode.UnknownError, exceptionMessage);
        result.Payload.Should().BeNull();
    }
}
