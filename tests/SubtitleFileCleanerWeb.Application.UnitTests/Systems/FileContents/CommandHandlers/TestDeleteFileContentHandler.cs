using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.CommandHandlers;
using SubtitleFileCleanerWeb.Application.FileContents.Commands;
using SubtitleFileCleanerWeb.Application.UnitTests.Fixtures.AutoData;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.Reflection;
using SubtitleFileCleanerWeb.Infrastructure.Blob;
using SubtitleFileCleanerWeb.Infrastructure.Exceptions;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.FileContents.CommandHandlers;

public class TestDeleteFileContentHandler
{
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;
    private readonly IBlobStorageContext _blobContextMock;
    private readonly DeleteFileContentHandler _sut;

    public TestDeleteFileContentHandler()
    {
        _blobContextMock = Substitute.For<IBlobStorageContext>();
        _sut = new(_blobContextMock);
    }

    [Theory, PathAutoData]
    public async Task Handle_WithValidPath_ReturnTrueResult
        (DeleteFileContent request)
    {
        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            
            .Which.Should().BeTrue();
    }

    [Theory, PathAutoData]
    public async Task Handle_WithBlobStorageOperationException_ReturnBlobContextOperationExceptionError
        (DeleteFileContent request, string message)
    {
        // Arrange
        var exception = InnerExceptionsCreator.Create<BlobStorageOperationException>(message);

        _blobContextMock
            .DeleteContentAsync(
                request.Path,
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

    [Theory, PathAutoData]
    public async Task Handle_WithUnexpectedException_ReturnUnknownError
        (DeleteFileContent request, string message)
    {
        // Arrange
        _blobContextMock
            .DeleteContentAsync(
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
