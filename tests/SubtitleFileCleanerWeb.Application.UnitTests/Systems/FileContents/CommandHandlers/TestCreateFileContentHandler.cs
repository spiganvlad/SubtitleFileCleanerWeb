using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.CommandHandlers;
using SubtitleFileCleanerWeb.Application.FileContents.Commands;
using SubtitleFileCleanerWeb.Application.UnitTests.Fixtures.AutoData;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.Reflection;
using SubtitleFileCleanerWeb.Infrastructure.Blob;
using SubtitleFileCleanerWeb.Infrastructure.Exceptions;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.FileContents.CommandHandlers;

public class TestCreateFileContentHandler
{
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;
    private readonly IBlobStorageContext _blobContextMock;
    private readonly CreateFileContentHandler _sut;

    public TestCreateFileContentHandler()
    {
        _blobContextMock = Substitute.For<IBlobStorageContext>();
        _sut = new(_blobContextMock);
    }

    [Theory, PathStreamAutoData]
    public async Task Handle_WithValidRequest_ReturnFileContentResult
        (CreateFileContent request)
    {
        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()

            .Which.Content.Should().NotBeNull()
            .And.BeReadOnly()
            .And.HaveLength(request.ContentStream.Length);
    }

    [Theory, PathStreamAutoData]
    public async Task Handle_WithInvalidContentStream_ReturnValidationError
        (CreateFileContent request)
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

    [Theory, PathStreamAutoData]
    public async Task Handle_WithBlobStorageOperationException_ReturnBlobContextOperationExceptionError
        (CreateFileContent request, string message)
    {
        // Arrange
        var exception = InnerExceptionsCreator.Create<BlobStorageOperationException>(message);

        _blobContextMock
            .CreateContentAsync(
                Arg.Any<string>(),
                request.ContentStream,
                Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.BlobContextOperationException, message)
            .And.HaveDefaultPayload();
    }

    [Theory, PathStreamAutoData]
    public async Task Handle_WithUnexpectedError_ReturnUnknownError
        (CreateFileContent request, string message)
    {
        // Arrange
        _blobContextMock
            .CreateContentAsync(
                Arg.Any<string>(),
                Arg.Any<Stream>(),
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
