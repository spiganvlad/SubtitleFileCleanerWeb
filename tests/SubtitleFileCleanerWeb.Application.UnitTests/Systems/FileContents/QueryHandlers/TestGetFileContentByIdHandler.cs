using FluentAssertions;
using Moq;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.Queries;
using SubtitleFileCleanerWeb.Application.FileContents.QueryHandlers;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.FluentOperationResult;
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
        var fileContextId = Guid.NewGuid();
        var contentStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5}, false);

        _blobContextMock.Setup(bc => bc.GetContentStreamAsync(fileContextId.ToString()))
            .ReturnsAsync(contentStream);
        var handler = new GetFileContentByIdHandler(_blobContextMock.Object);

        var request = new GetFileContentById(fileContextId);
        var cancellationToken = new CancellationToken();

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _blobContextMock.Verify(bc => bc.GetContentStreamAsync(It.IsAny<string>()), Times.Once);

        result.Should().NotBeNull().And.ContainsNoErrors();
        result.Payload.Should().NotBeNull();
        result.Payload!.Content.Should().NotBeNull().And.BeReadOnly();
        result.Payload.Content.Length.Should().Be(contentStream.Length);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnNotFoundError()
    {
        // Arrange
        _blobContextMock.Setup(bc => bc.GetContentStreamAsync(It.IsAny<string>()))
            .ReturnsAsync(() => null);
        var handler = new GetFileContentByIdHandler(_blobContextMock.Object);

        var request = new GetFileContentById(Guid.Empty);
        var cancellationToken = new CancellationToken();

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _blobContextMock.Verify(bc => bc.GetContentStreamAsync(It.IsAny<string>()), Times.Once());

        result.Should().NotBeNull().And
            .ContainSingleError(ErrorCode.NotFound, $"No file content found with id: {Guid.Empty}");
    }

    [Fact]
    public async Task Handle_WithWritableStreamFromBlob_ReturnValidationError()
    {
        // Arrange
        _blobContextMock.Setup(bc => bc.GetContentStreamAsync(Guid.Empty.ToString()))
            .ReturnsAsync(() => new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }, true));
        var handler = new GetFileContentByIdHandler(_blobContextMock.Object);

        var request = new GetFileContentById(Guid.Empty);
        var cancellationToken = new CancellationToken();

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _blobContextMock.Verify(bc => bc.GetContentStreamAsync(It.IsAny<string>()), Times.Once());

        result.Should().NotBeNull().And.
            ContainSingleError(ErrorCode.ValidationError, "File content stream must be readonly");
    }
}
