using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.Queries;
using SubtitleFileCleanerWeb.Application.FileContents.QueryHandlers;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Infrastructure.Blob;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.FileContents.QueryHandlers;

public class TestGetFileContentByIdHandler
{
    private readonly Mock<IBlobStorageContext> _blobContextMock;

    public TestGetFileContentByIdHandler()
    {
        _blobContextMock = new();
    }

    [Fact]
    public async Task Handle_WithValidParameters_ReturnValid()
    {
        // Arrange
        var contentStream = new MemoryStream(new byte[] { 1 }, false);

        _blobContextMock.Setup(
            bc => bc.GetContentStreamAsync(
                string.Empty,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(contentStream);

        var request = new GetFileContentById(string.Empty);

        var handler = new GetFileContentByIdHandler(_blobContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _blobContextMock.Verify(
            bc => bc.GetContentStreamAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            
            .Which.Content.Should().NotBeNull()
            .And.BeReadOnly()
            .And.HaveLength(contentStream.Length);
    }

    [Fact]
    public async Task Handle_WithBlobContextReturnsNullContent_ReturnNotFoundError()
    {
        // Arrange
        _blobContextMock.Setup(
            bc => bc.GetContentStreamAsync(
                string.Empty,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null);

        var request = new GetFileContentById(string.Empty);

        var handler = new GetFileContentByIdHandler(_blobContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _blobContextMock.Verify(
            bc => bc.GetContentStreamAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.NotFound, $"File content not found on path: {string.Empty}.")
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task Handle_WithBlobContextReturnsInvalidContent_ReturnValidationError()
    {
        // Arrange
        var contentStream = new MemoryStream(Array.Empty<byte>(), true);

        _blobContextMock.Setup(
            bc => bc.GetContentStreamAsync(
                string.Empty,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(contentStream);

        var request = new GetFileContentById(string.Empty);

        var handler = new GetFileContentByIdHandler(_blobContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _blobContextMock.Verify(
            bc => bc.GetContentStreamAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveMultipleErrors(
                new Error
                {
                    Code = ErrorCode.ValidationError,
                    Message = "File content stream cannot be empty."
                },
                new Error
                {
                    Code = ErrorCode.ValidationError,
                    Message = "File content stream must be readonly."
                })
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task Handle_WithUnexpectedError_ReturnUnknownError()
    {
        // Arrange
        var exceptionMessage = "Test unexpected error occurred.";
        _blobContextMock.Setup(
            bc => bc.GetContentStreamAsync(
                string.Empty,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        var request = new GetFileContentById(string.Empty);

        var handler = new GetFileContentByIdHandler(_blobContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _blobContextMock.Verify(
            bc => bc.GetContentStreamAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, exceptionMessage)
            .And.HaveDefaultPayload();
    }
}
