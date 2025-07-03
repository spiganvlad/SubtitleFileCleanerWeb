using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.Queries;
using SubtitleFileCleanerWeb.Application.FileContents.QueryHandlers;
using SubtitleFileCleanerWeb.Application.UnitTests.Fixtures.AutoData;
using SubtitleFileCleanerWeb.Infrastructure.Blob;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.FileContents.QueryHandlers;

public class TestGetFileContentByIdHandler
{
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;
    private readonly IBlobStorageContext _blobContextMock;
    private readonly GetFileContentByIdHandler _sut;

    public TestGetFileContentByIdHandler()
    {
        _blobContextMock = Substitute.For<IBlobStorageContext>();
        _sut = new(_blobContextMock);
    }

    [Theory, PathStreamAutoData]
    public async Task Handle_WithValidPath_ReturnFileContentResult
        (GetFileContentById request, Stream contentStream)
    {
        // Arrange
        _blobContextMock
            .GetContentStreamAsync(
                request.Path,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Stream?>(contentStream));

        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            
            .Which.Content.Should().NotBeNull()
            .And.BeReadOnly()
            .And.HaveLength(contentStream.Length);
    }

    [Theory, PathAutoData]
    public async Task Handle_WithBlobContextReturnsNullContent_ReturnNotFoundError
        (GetFileContentById request)
    {
        // Arrange
        _blobContextMock
            .GetContentStreamAsync(
                request.Path,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Stream?>(null));

        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(
                ErrorCode.NotFound,
                $"File content not found on path: {request.Path}.")
            .And.HaveDefaultPayload();
    }

    [Theory, PathAutoData]
    public async Task Handle_WithBlobContextReturnsInvalidContent_ReturnValidationError
        (GetFileContentById request)
    {
        // Arrange
        var invalidContentStream = new MemoryStream([], true);
        _blobContextMock
            .GetContentStreamAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Stream?>(invalidContentStream));

         // Act
         var result = await _sut.Handle(request, _cancellationToken);

         // Assert
         result.Should().NotBeNull()
             .And.BeInErrorState()
             .And.HaveAllErrorsWithCode(ErrorCode.ValidationError)
             .And.HaveDefaultPayload();
     }

     [Theory, PathAutoData]
     public async Task Handle_WithUnexpectedError_ReturnUnknownError
        (GetFileContentById request, string message)
     {
        // Arrange
        _blobContextMock
            .GetContentStreamAsync(
                Arg.Any<string>(),
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
