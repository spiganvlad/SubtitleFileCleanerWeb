using FluentAssertions;
using Moq;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.CommandHandlers;
using SubtitleFileCleanerWeb.Application.FileContents.Commands;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.Reflection;
using SubtitleFileCleanerWeb.Infrastructure.Blob;
using SubtitleFileCleanerWeb.Infrastructure.Exceptions;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.FileContents.CommandHandlers;

public class TestCreateFileContentHandler
{
    private readonly Mock<IBlobStorageContext> _blobContextMock;

    public TestCreateFileContentHandler()
    {
        _blobContextMock = new();
    }

    [Fact]
    public async Task Handle_WithValidParameters_ReturnValid()
    {
        // Arrange
        var streamContent = new MemoryStream(new byte[] { 1 }, false);

        var request = new CreateFileContent(string.Empty, streamContent);

        var handler = new CreateFileContentHandler(_blobContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _blobContextMock.Verify(
            bc => bc.CreateContentAsync(
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            
            .Which.Content.Should().NotBeNull()
            .And.BeReadOnly()
            .And.HaveLength(streamContent.Length);
    }

    [Fact]
    public async Task Handle_WithInvalidParameters_ReturnValidationError()
    {
        // Arrange
        var request = new CreateFileContent(string.Empty, null!);

        var handler = new CreateFileContentHandler(_blobContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _blobContextMock.Verify(
            bc => bc.CreateContentAsync(
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()),
            Times.Never());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.ValidationError, "File content stream cannot be null.")
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task Handle_WithBlobStorageOperationException_ReturnBlobContextOperationExceptionError()
    {
        // Arrange
        var contentStream = new MemoryStream(new byte[] { 1 }, false);

        var exceptionMessage = "Blob exception occurred";
        var exception = InnerExceptionsCreator.Create<BlobStorageOperationException>(exceptionMessage);

        _blobContextMock.Setup(
            bc => bc.CreateContentAsync(
                string.Empty,
                contentStream,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var request = new CreateFileContent(string.Empty, contentStream);

        var handler = new CreateFileContentHandler(_blobContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _blobContextMock.Verify(
            bc => bc.CreateContentAsync(
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.BlobContextOperationException, exceptionMessage)
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task Handle_WithUnexpectedError_ReturnUnknownError()
    {
        // Arrange
        var contentStream = new MemoryStream(new byte[] { 1 }, false);

        var exceptionMessage = "Unexpected exception occurred";
        _blobContextMock.Setup(
            bc => bc.CreateContentAsync(
                string.Empty,
                contentStream,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        var request = new CreateFileContent(string.Empty, contentStream);

        var handler = new CreateFileContentHandler(_blobContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _blobContextMock.Verify(
            bc => bc.CreateContentAsync(
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, exceptionMessage)
            .And.HaveDefaultPayload();
    }
}
