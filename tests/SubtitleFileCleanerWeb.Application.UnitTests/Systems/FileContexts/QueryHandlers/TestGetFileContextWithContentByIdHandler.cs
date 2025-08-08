using AutoFixture.Xunit3;
using MockQueryable.NSubstitute;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.Queries;
using SubtitleFileCleanerWeb.Application.FileContexts.Queries;
using SubtitleFileCleanerWeb.Application.FileContexts.QueryHandlers;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.UnitTests.Fixtures.AutoData;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.FileContexts.QueryHandlers;

public class TestGetFileContextWithContentByIdHandler
{
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;
    private readonly IMediator _mediatorMock;
    private readonly ApplicationDbContext _dbContextMock;
    private readonly GetFileContextWithContentByIdHandler _sut;

    public TestGetFileContextWithContentByIdHandler()
    {
        _mediatorMock = Substitute.For<IMediator>();
        _dbContextMock = Substitute.For<ApplicationDbContext>();
        _sut = new GetFileContextWithContentByIdHandler(_mediatorMock, _dbContextMock);
    }

    [Theory, StreamAutoData]
    public async Task Handle_WithExistingId_ReturnFileContextResult
        (List<FileContext> fileContexts, OperationResult<FileContent> searchedContent)
    {
        // Arrange
        var searchedContext = fileContexts.Last();
        var path = Path.Combine("Unauthorized", searchedContext.FileContextId.ToString());

        var fileContextsDbSet = fileContexts.AsQueryable().BuildMockDbSet();
        _dbContextMock.FileContexts.Returns(fileContextsDbSet);

        _mediatorMock
            .Send(
                Arg.Is<GetFileContentById>(x => x.Path == path),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(searchedContent));

        var request = new GetFileContextWithContentById(searchedContext.FileContextId);

        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        var payload = result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            .Which;

        payload.Should().Be(searchedContext);
        payload.FileContent.Should().Be(searchedContent.Payload);
    }

    [Theory, AutoData]
    public async Task Handle_WithNonExistentId_ReturnNotFoundError
        (GetFileContextWithContentById request)
    {
        // Arrange
        var fileContextsDbSet = Enumerable.Empty<FileContext>().AsQueryable().BuildMockDbSet();
        _dbContextMock.FileContexts.Returns(fileContextsDbSet);

        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(
                ErrorCode.NotFound,
                $"No file context found with id: {request.FileContextId}.")
            .And.HaveDefaultPayload();
    }

    [Theory, AutoData]
    public async Task Handle_WithFileContentError_RaiseError
        (List<FileContext> fileContexts, ErrorCode code, string message)
    {
        // Arrange
        var searchedFileContext = fileContexts.Last();
        var path = Path.Combine("Unauthorized", searchedFileContext.FileContextId.ToString());
        var request = new GetFileContextWithContentById(searchedFileContext.FileContextId);

        var fileContextsDbSet = fileContexts.AsQueryable().BuildMockDbSet();
        _dbContextMock.FileContexts.Returns(fileContextsDbSet);

        var fileContentResult = new OperationResult<FileContent>();
        fileContentResult.AddError(code, message);

        _mediatorMock
            .Send(
                Arg.Is<GetFileContentById>(x => x.Path == path),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(fileContentResult));

        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(code, message)
            .And.HaveDefaultPayload();
    }

    [Theory, AutoData]
    public async Task Handle_WithUnexpectedException_ReturnUnknownError
        (GetFileContextWithContentById request, string message)
    {
        // Arrange
        _dbContextMock.FileContexts.Throws(new Exception(message));

        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, message)
            .And.HaveDefaultPayload();
    }
}
