using FluentAssertions;
using Moq;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.Queries;
using SubtitleFileCleanerWeb.Application.FileContents.QueryHandlers;
using SubtitleFileCleanerWeb.Infrastructure.Blob;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.FileContents.QueryHandlers;

public class TestGetFileContentByIdHandler
{
    private readonly Mock<IBlobStorageContext> _blobContextMock;

    public TestGetFileContentByIdHandler()
    {
        _blobContextMock = new Mock<IBlobStorageContext>();
    }

    [Fact]
    public async Task Handle_WithExistingId_ReturnValid()
    {
        // Arrange
        var contentStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5}, false);
        var cancellationToken = new CancellationToken();

        _blobContextMock.Setup(bc => bc.GetContentStreamAsync(string.Empty, cancellationToken))
            .ReturnsAsync(contentStream);

        var request = new GetFileContentById(string.Empty);

        var handler = new GetFileContentByIdHandler(_blobContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _blobContextMock.Verify(bc => bc.GetContentStreamAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            
            .Which.Content.Should().NotBeNull()
            .And.BeReadOnly()
            .And.HaveLength(contentStream.Length);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnNotFoundError()
    {
        // Arrange
        var cancellationToken = new CancellationToken();

        _blobContextMock.Setup(bc => bc.GetContentStreamAsync(string.Empty, cancellationToken))
            .ReturnsAsync(() => null);

        var request = new GetFileContentById(string.Empty);

        var handler = new GetFileContentByIdHandler(_blobContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _blobContextMock.Verify(bc => bc.GetContentStreamAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.NotFound, $"File content not found on path: {string.Empty}")
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task Handle_WithWritableStreamFromBlob_ReturnValidationError()
    {
        // Arrange
        var cancellationToken = new CancellationToken();

        _blobContextMock.Setup(bc => bc.GetContentStreamAsync(string.Empty, cancellationToken))
            .ReturnsAsync(() => new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }, true));

        var request = new GetFileContentById(string.Empty);

        var handler = new GetFileContentByIdHandler(_blobContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _blobContextMock.Verify(bc => bc.GetContentStreamAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.ValidationError, "File content stream must be readonly.")
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task Handle_WithUnexpectedError_ReturnUnknownError()
    {
        // Arrange

        var cancellationToken = new CancellationToken();

        var exceptionMessage = "Unexpected error occurred";
        _blobContextMock.Setup(bc => bc.GetContentStreamAsync(string.Empty, cancellationToken))
            .ThrowsAsync(new Exception(exceptionMessage));

        var request = new GetFileContentById(string.Empty);

        var handler = new GetFileContentByIdHandler(_blobContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _blobContextMock.Verify(bc => bc.GetContentStreamAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, exceptionMessage)
            .And.HaveDefaultPayload();
    }
}
