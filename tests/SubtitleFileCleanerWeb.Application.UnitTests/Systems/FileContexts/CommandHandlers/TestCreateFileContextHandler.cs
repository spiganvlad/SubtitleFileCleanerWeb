using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.Commands;
using SubtitleFileCleanerWeb.Application.FileContexts.CommandHandlers;
using SubtitleFileCleanerWeb.Application.FileContexts.Commands;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.UnitTests.Fixtures.AutoData;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.FileContexts.CommandHandlers;

public class TestCreateFileContextHandler
{
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;
    private readonly ApplicationDbContext _dbContextMock;
    private readonly IMediator _mediatorMock;
    private readonly CreateFileContextHandler _sut;

    public TestCreateFileContextHandler()
    {
        _dbContextMock = Substitute.For<ApplicationDbContext>();
        _mediatorMock = Substitute.For<IMediator>();
        _sut = new(_mediatorMock, _dbContextMock);
    }

    [Theory, StreamAutoData]
    public async Task Handle_WithValidRequest_ReturnFileContextResult
        (CreateFileContext request, OperationResult<FileContent> mediatorResult)
    {
        // Arrange
        _mediatorMock
            .Send(
                Arg.Is<CreateFileContent>(x => x.ContentStream == request.ContentStream),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(mediatorResult));

        FileContext savedFileContext = null!;
        _dbContextMock.FileContexts.Add(Arg.Do<FileContext>(fileContext =>
        {
            savedFileContext = fileContext;
        }));

        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        var payload = result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            .Which;

        payload.FileContextId.Should().NotBeEmpty();
        payload.Name.Should().Be(request.FileName + ".txt");
        payload.FileContent.Should().Be(mediatorResult.Payload);
        payload.ContentSize.Should().Be(mediatorResult.Payload!.Content.Length);
        payload.DateCreated.Should().BeCloseTo(DateTime.UtcNow, 1.Seconds());
        payload.DateModified.Should().BeCloseTo(DateTime.UtcNow, 1.Seconds());

        savedFileContext.Should().Be(payload);
        await _dbContextMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory, StreamAutoData]
    public async Task Handle_WithInvalidContentStream_ReturnValidationError
        (CreateFileContext request)
    {
        // Arrange
        var invalidRequest = request with { ContentStream = new MemoryStream([], true) };

        // Act
        var result = await _sut.Handle(invalidRequest, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveAllErrorsWithCode(ErrorCode.ValidationError)
            .And.HaveDefaultPayload();
    }

    [Theory, StreamAutoData]
    public async Task Handle_WithCreateContentError_RaiseError
        (CreateFileContext request, ErrorCode code, string message)
    {
        // Arrange
        var mediatorResult = new OperationResult<FileContent>();
        mediatorResult.AddError(code, message);

        _mediatorMock
            .Send(
                Arg.Is<CreateFileContent>(x => x.ContentStream == request.ContentStream),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(mediatorResult));

        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(code, message)
            .And.HaveDefaultPayload();
    }

    [Theory, StreamAutoData]
    public async Task Handle_WithUnexpectedError_ReturnUnknownError
        (CreateFileContext request, string message)
    {
        // Arrange
        _mediatorMock
            .Send(
                Arg.Any<CreateFileContent>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(message));

        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, message)
            .And.HaveDefaultPayload();
    }
}
