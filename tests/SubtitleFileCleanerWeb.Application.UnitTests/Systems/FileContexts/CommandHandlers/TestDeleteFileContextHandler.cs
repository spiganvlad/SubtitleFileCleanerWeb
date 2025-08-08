using AutoFixture.Xunit3;
using MockQueryable.NSubstitute;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.Commands;
using SubtitleFileCleanerWeb.Application.FileContexts.CommandHandlers;
using SubtitleFileCleanerWeb.Application.FileContexts.Commands;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.FileContexts.CommandHandlers;

public class TestDeleteFileContextHandler
{
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;
    private readonly IMediator _mediatorMock;
    private readonly ApplicationDbContext _dbContextMock;
    private readonly DeleteFileContextHandler _sut;

    public TestDeleteFileContextHandler()
    {
        _mediatorMock = Substitute.For<IMediator>();
        _dbContextMock = Substitute.For<ApplicationDbContext>();
        _sut = new(_mediatorMock, _dbContextMock);
    }

    [Theory, AutoData]
    public async Task Handle_WithValidGuidId_ReturnFileContextResult
        (List<FileContext> fileContexts)
    {
        // Arrange
        var fileContextsDbSet = fileContexts.AsQueryable().BuildMockDbSet();
        var contextToDelete = fileContexts.Last();

        var request = new DeleteFileContext(contextToDelete.FileContextId);
        var path = Path.Combine("Unauthorized", contextToDelete.FileContextId.ToString());

        _mediatorMock
            .Send(
                Arg.Is<DeleteFileContent>(x => x.Path == path),
                Arg.Any<CancellationToken>())
            .Returns(new OperationResult<bool> { Payload = true });

        
        _dbContextMock.FileContexts.Returns(fileContextsDbSet);
        _dbContextMock.FileContexts.Remove(Arg.Do<FileContext>(x =>
            fileContexts.Remove(x)));

        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            
            .Which.Should().Be(contextToDelete);

        fileContexts.Should().HaveCount(2)
            .And.NotContain(contextToDelete);

        await _dbContextMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory, AutoData]
    public async Task Handle_WithNonExistentGuidId_ReturnNotFoundError
        (DeleteFileContext request)
    {
        // Arrange
        var fileContextsDbSet = Array.Empty<FileContext>().AsQueryable().BuildMockDbSet();
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
    public async Task Handle_WithContentDeleteError_RaiseError
        (List<FileContext> fileContexts, ErrorCode code, string message)
    {
        // Arrange
        var fileContextsDbSet = fileContexts.AsQueryable().BuildMockDbSet();
        var contextToDelete = fileContexts.Last();

        var path = Path.Combine("Unauthorized", contextToDelete.FileContextId.ToString());
        var request = new DeleteFileContext(contextToDelete.FileContextId);

        var fileContentResult = new OperationResult<bool>();
        fileContentResult.AddError(code, message);

        _mediatorMock
            .Send(
                Arg.Is<DeleteFileContent>(x => x.Path == path),
                Arg.Any<CancellationToken>())
            .Returns(fileContentResult);

        _dbContextMock.FileContexts.Returns(fileContextsDbSet);

        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(code, message)
            .And.HaveDefaultPayload();
    }

    [Theory, AutoData]
    public async Task Handle_WithUnknownError_ReturnUnknownError
        (DeleteFileContext request, string message)
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
