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

public class TestDeleteFileContentHandler
{
    private readonly Mock<IBlobStorageContext> _storageContext;

    public TestDeleteFileContentHandler()
    {
        _storageContext = new Mock<IBlobStorageContext>();
    }

    [Fact]
    public async Task Handle_WithExistedContent_DeleteValid()
    {
        // Arrange
        var cancellationToken = new CancellationToken();

        _storageContext.Setup(sc => sc.DeleteContentAsync(string.Empty, cancellationToken));

        var request = new DeleteFileContent(string.Empty);

        var handler = new DeleteFileContentHandler(_storageContext.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _storageContext.Verify(sc => sc.DeleteContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull().And.ContainsNoErrors();
        result.Payload.Should().Be(true);
    }

    [Fact]
    public async Task Handle_WithBlobStorageOperationException_ReturnBlobContextOperationExceptionError()
    {
        // Arrange
        var cancellationToken = new CancellationToken();

        var exceptionMessage = "Test exception occurred";
        var exception = InnerExceptionsCreator.Create<BlobStorageOperationException>(exceptionMessage);
        _storageContext.Setup(sc => sc.DeleteContentAsync(string.Empty, cancellationToken))
            .ThrowsAsync(exception);

        var request = new DeleteFileContent(string.Empty);

        var handler = new DeleteFileContentHandler(_storageContext.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _storageContext.Verify(sc => sc.DeleteContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull()
            .And.ContainSingleError(ErrorCode.BlobContextOperationException, exceptionMessage);
        result.Payload.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithUnexpectedException_ReturnUnknownError()
    {
        // Arrange
        var cancellationToken = new CancellationToken();

        var exceptionMessage = "Test unexpected error occurred.";
        _storageContext.Setup(sc => sc.DeleteContentAsync(string.Empty, cancellationToken))
            .Throws(new Exception(exceptionMessage));

        var request = new DeleteFileContent(string.Empty);

        var handler = new DeleteFileContentHandler(_storageContext.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        result.Should().NotBeNull().And.ContainSingleError(ErrorCode.UnknownError, exceptionMessage);
        result.Payload.Should().Be(false);
    }
}
