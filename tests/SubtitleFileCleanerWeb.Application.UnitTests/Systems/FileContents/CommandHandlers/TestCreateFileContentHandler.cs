using FluentAssertions;
using Moq;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.CommandHandlers;
using SubtitleFileCleanerWeb.Application.FileContents.Commands;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.FluentOperationResult;
using SubtitleFileCleanerWeb.Infrastructure.Blob;

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
        var handler = new CreateFileContentHandler(_blobContextMock.Object);

        var streamContent = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }, false);
        var request = new CreateFileContent(Guid.NewGuid(), streamContent);
        var cancellationToken = new CancellationToken();

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _blobContextMock.Verify(b => b.CreateContentAsync(It.IsAny<string>(), It.IsAny<Stream>()), Times.Once());

        result.Should().NotBeNull().And.ContainsNoErrors();
        result.Payload.Should().NotBeNull();
        result.Payload!.Content.Should().BeReadOnly();
        result.Payload.Content.Length.Should().Be(streamContent.Length);
    }

    [Fact]
    public async Task Handle_WithNullStream_ReturnValidationError()
    {
        // Arrange
        var handler = new CreateFileContentHandler(_blobContextMock.Object);

        var request = new CreateFileContent(Guid.Empty, null!);
        var cancellationToken = new CancellationToken();

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _blobContextMock.Verify(bc => bc.CreateContentAsync(It.IsAny<string>(), It.IsAny<Stream>()), Times.Never());

        result.Should().ContainSingleError(ErrorCode.ValidationError, "File content stream cannot be null");
    }

    [Fact]
    public async Task Handle_WithEmptyStream_ReturnValidationError()
    {
        // Arrange
        var handler = new CreateFileContentHandler(_blobContextMock.Object);

        var request = new CreateFileContent(Guid.Empty, new MemoryStream(Array.Empty<byte>()));
        var cancellationToken = new CancellationToken();

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _blobContextMock.Verify(bc => bc.CreateContentAsync(It.IsAny<string>(), It.IsAny<Stream>()), Times.Never());

        result.Should().NotBeNull().And
            .ContainSingleError(ErrorCode.ValidationError, "File content stream cannot be empty");
    }

    [Fact]
    public async Task Handle_WithWritableStream_ReturnValidationError()
    {
        // Arrange
        var handler = new CreateFileContentHandler(_blobContextMock.Object);

        var contentStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }, true);
        var request = new CreateFileContent(Guid.Empty, contentStream);
        var cancellationToken = new CancellationToken();

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _blobContextMock.Verify(bc => bc.CreateContentAsync(It.IsAny<string>(), It.IsAny<Stream>()), Times.Never());

        result.Should().NotBeNull().And
            .ContainSingleError(ErrorCode.ValidationError, "File content stream must be readonly");
    }
}
