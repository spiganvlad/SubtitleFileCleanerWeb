using FluentAssertions;
using Moq;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.CommandHandlers;
using SubtitleFileCleanerWeb.Application.FileContents.Commands;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.Extensions;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.Reflection;
using SubtitleFileCleanerWeb.Infrastructure.Blob;
using SubtitleFileCleanerWeb.Infrastructure.Exceptions;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.FileContents.CommandHandlers;

public class TestCreateFileContentHandler
{
    private readonly Mock<IBlobStorageContext> _blobContextMock;

    public TestCreateFileContentHandler()
    {
        _blobContextMock = new Mock<IBlobStorageContext>();
    }

    [Fact]
    public async Task Handle_WithValidParameters_ReturnValid()
    {
        // Arrange
        var streamContent = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }, false);
        var cancellationToken = new CancellationToken();

        var request = new CreateFileContent(string.Empty, streamContent);

        var handler = new CreateFileContentHandler(_blobContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _blobContextMock.Verify(bc => bc.CreateContentAsync(It.IsAny<string>(), It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull().And.ContainsNoErrors();
        result.Payload.Should().NotBeNull();
        result.Payload!.Content.Should().BeReadOnly().And.HaveLength(streamContent.Length);
    }

    [Fact]
    public async Task Handle_WithNullStream_ReturnValidationError()
    {
        // Arrange
        var cancellationToken = new CancellationToken();

        var request = new CreateFileContent(string.Empty, null!);

        var handler = new CreateFileContentHandler(_blobContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _blobContextMock.Verify(bc => bc.CreateContentAsync(It.IsAny<string>(), It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()), Times.Never());

        result.Should().ContainSingleError(ErrorCode.ValidationError, "File content stream cannot be null.");
        result.Payload.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithEmptyStream_ReturnValidationError()
    {
        // Arrange
        var cancellationToken = new CancellationToken();

        var request = new CreateFileContent(string.Empty, new MemoryStream(Array.Empty<byte>()));

        var handler = new CreateFileContentHandler(_blobContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _blobContextMock.Verify(bc => bc.CreateContentAsync(It.IsAny<string>(), It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()), Times.Never());

        result.Should().NotBeNull().And
            .ContainSingleError(ErrorCode.ValidationError, "File content stream cannot be empty.");
        result.Payload.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithWritableStream_ReturnValidationError()
    {
        // Arrange
        var contentStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }, true);
        var cancellationToken = new CancellationToken();

        var request = new CreateFileContent(string.Empty, contentStream);

        var handler = new CreateFileContentHandler(_blobContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _blobContextMock.Verify(bc => bc.CreateContentAsync(It.IsAny<string>(), It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()), Times.Never());

        result.Should().NotBeNull().And
            .ContainSingleError(ErrorCode.ValidationError, "File content stream must be readonly.");
        result.Payload.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithBlobStorageOperationException_ReturnBlobContextOperationExceptionError()
    {
        // Arrange
        var contentStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }, false);
        var cancellationToken = new CancellationToken();

        var exceptionMessage = "Blob exception occurred";
        var exception = InnerExceptionsCreator.Create<BlobStorageOperationException>(exceptionMessage);
        _blobContextMock.Setup(bc => bc.CreateContentAsync(string.Empty, contentStream, cancellationToken))
            .ThrowsAsync(exception);

        var request = new CreateFileContent(string.Empty, contentStream);

        var handler = new CreateFileContentHandler(_blobContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _blobContextMock.Verify(bc => bc.CreateContentAsync(It.IsAny<string>(), It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull().And.ContainSingleError(ErrorCode.BlobContextOperationException, exceptionMessage);
        result.Payload.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithUnexpectedError_ReturnUnknownError()
    {
        // Arrange
        var contentStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }, false);
        var cancellationToken = new CancellationToken();

        var exceptionMessage = "Unexpected exception occurred";
        _blobContextMock.Setup(bc => bc.CreateContentAsync(string.Empty, contentStream, cancellationToken))
            .ThrowsAsync(new Exception(exceptionMessage));

        var request = new CreateFileContent(string.Empty, contentStream);

        var handler = new CreateFileContentHandler(_blobContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _blobContextMock.Verify(bc => bc.CreateContentAsync(It.IsAny<string>(), It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull().And.ContainSingleError(ErrorCode.UnknownError, exceptionMessage);
        result.Payload.Should().BeNull();
    }
}
