﻿using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.CommandHandlers;
using SubtitleFileCleanerWeb.Application.FileContents.Commands;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.Reflection;
using SubtitleFileCleanerWeb.Infrastructure.Blob;
using SubtitleFileCleanerWeb.Infrastructure.Exceptions;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.FileContents.CommandHandlers;

public class TestDeleteFileContentHandler
{
    private readonly Mock<IBlobStorageContext> _storageContext;

    public TestDeleteFileContentHandler()
    {
        _storageContext = new();
    }

    [Fact]
    public async Task Handle_WithExistedContent_DeleteValid()
    {
        // Arrange
        _storageContext.Setup(sc => sc.DeleteContentAsync(
            string.Empty,
            It.IsAny<CancellationToken>()));

        var request = new DeleteFileContent(string.Empty);

        var handler = new DeleteFileContentHandler(_storageContext.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _storageContext.Verify(
            sc => sc.DeleteContentAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload();
    }

    [Fact]
    public async Task Handle_WithBlobStorageOperationException_ReturnBlobContextOperationExceptionError()
    {
        // Arrange
        var exceptionMessage = "Test blob exception occurred.";
        var exception = InnerExceptionsCreator.Create<BlobStorageOperationException>(exceptionMessage);

        _storageContext.Setup(
            sc => sc.DeleteContentAsync(
                string.Empty,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var request = new DeleteFileContent(string.Empty);

        var handler = new DeleteFileContentHandler(_storageContext.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _storageContext.Verify(
            sc => sc.DeleteContentAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.BlobContextOperationException, exceptionMessage)
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task Handle_WithUnexpectedException_ReturnUnknownError()
    {
        // Arrange
        var exceptionMessage = "Test unexpected error occurred.";
        _storageContext.Setup(
            sc => sc.DeleteContentAsync(
                string.Empty,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        var request = new DeleteFileContent(string.Empty);

        var handler = new DeleteFileContentHandler(_storageContext.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _storageContext.Verify(
            sc => sc.DeleteContentAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, exceptionMessage)
            .And.HaveDefaultPayload();
    }
}
