using FluentAssertions;
using Moq;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.Queries;
using SubtitleFileCleanerWeb.Application.FileContents.QueryHandlers;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.Extensions;
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

        var request = new GetFileContentById(Guid.NewGuid());
        var cancellationToken = new CancellationToken();

        _blobContextMock.Setup(bc => bc.GetContentStreamAsync(request.FileContextId.ToString(), cancellationToken))
            .ReturnsAsync(contentStream);
        var handler = new GetFileContentByIdHandler(_blobContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _blobContextMock.Verify(bc => bc.GetContentStreamAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

        result.Should().NotBeNull().And.ContainsNoErrors();
        result.Payload.Should().NotBeNull();
        result.Payload!.Content.Should().NotBeNull().And.BeReadOnly();
        result.Payload.Content.Length.Should().Be(contentStream.Length);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnNotFoundError()
    {
        // Arrange
        var request = new GetFileContentById(Guid.Empty);
        var cancellationToken = new CancellationToken();

        _blobContextMock.Setup(bc => bc.GetContentStreamAsync(request.FileContextId.ToString(), cancellationToken))
            .ReturnsAsync(() => null);
        var handler = new GetFileContentByIdHandler(_blobContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _blobContextMock.Verify(bc => bc.GetContentStreamAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull().And
            .ContainSingleError(ErrorCode.NotFound, $"No file content found with id: {Guid.Empty}");
        result.Payload.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithWritableStreamFromBlob_ReturnValidationError()
    {
        // Arrange
        var request = new GetFileContentById(Guid.Empty);
        var cancellationToken = new CancellationToken();

        _blobContextMock.Setup(bc => bc.GetContentStreamAsync(request.FileContextId.ToString(), cancellationToken))
            .ReturnsAsync(() => new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }, true));
        var handler = new GetFileContentByIdHandler(_blobContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _blobContextMock.Verify(bc => bc.GetContentStreamAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull().And.
            ContainSingleError(ErrorCode.ValidationError, "File content stream must be readonly");
        result.Payload.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithUnknownError_ReturnUnknownError()
    {
        // Arrange
        var request = new GetFileContentById(Guid.Empty);
        var cancellationToken = new CancellationToken();

        var exceptionMessage = "Unexpected error occurred";
        _blobContextMock.Setup(bc => bc.GetContentStreamAsync(request.FileContextId.ToString(), cancellationToken))
            .ThrowsAsync(new Exception(exceptionMessage));
        var handler = new GetFileContentByIdHandler(_blobContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _blobContextMock.Verify(bc => bc.GetContentStreamAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull().And.ContainSingleError(ErrorCode.UnknownError, exceptionMessage);
        result.Payload.Should().BeNull();
    }
}
