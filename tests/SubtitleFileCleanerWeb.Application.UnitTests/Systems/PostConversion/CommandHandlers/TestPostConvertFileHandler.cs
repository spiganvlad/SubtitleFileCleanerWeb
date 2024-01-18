using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.PostConversion.CommandHandlers;
using SubtitleFileCleanerWeb.Application.PostConversion.Commands;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.PostConversion.CommandHandlers;

public class TestPostConvertFileHandler
{
    private readonly Mock<IPostConversionProcessor> _processorMock;

    public TestPostConvertFileHandler()
    {
        _processorMock = new();
    }

    [Fact]
    public async Task Handle_WithValidParameters_ReturnValid()
    {
        // Arrange
        var contentStream = new MemoryStream(new byte[] { 1, 2 });
        var expectedContentStream = new MemoryStream(new byte[] { 1 }, false);

        var conversionOptions = new PostConversionOption[]
        {
            (PostConversionOption)(-1),
            (PostConversionOption)(-2)
        };

        var processorResult = new OperationResult<Stream>
        {
            Payload = expectedContentStream
        };

        _processorMock.Setup(
            p => p.ProcessAsync(
                contentStream,
                It.IsAny<CancellationToken>(),
                conversionOptions))
            .ReturnsAsync(processorResult);

        var request = new PostConvertFile(contentStream, conversionOptions);

        var handler = new PostConvertFileHandler(_processorMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _processorMock.Verify(
            p => p.ProcessAsync(
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<PostConversionOption[]>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            
            .Which.Should().BeReadOnly()
            .And.HaveLength(expectedContentStream.Length);
    }

    [Fact]
    public async Task Handle_WithProcessorError_RaiseError()
    {
        // Arrange
        var contentStream = Stream.Null;
        var conversionOptions = Array.Empty<PostConversionOption>();

        var processorResult = new OperationResult<Stream>();

        var errorCode = (ErrorCode)(-1);
        var errorMessage = "Test unexpected error occurred."; 
        processorResult.AddError(errorCode, errorMessage);

        _processorMock.Setup(
            p => p.ProcessAsync(
                contentStream,
                It.IsAny<CancellationToken>(),
                conversionOptions))
            .ReturnsAsync(processorResult);

        var request = new PostConvertFile(contentStream, conversionOptions);

        var handler = new PostConvertFileHandler(_processorMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _processorMock.Verify(
            p => p.ProcessAsync(
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<PostConversionOption[]>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(errorCode, errorMessage)
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task Handle_WithUnexpectedException_ReturnUnknowError()
    {
        // Arrange
        var contentStream = Stream.Null;
        var conversionOptions = Array.Empty<PostConversionOption>();

        var exceptionMessage = "Test unexpected exception occurred.";
        _processorMock.Setup(
            p => p.ProcessAsync(
                contentStream,
                It.IsAny<CancellationToken>(),
                conversionOptions))
            .ThrowsAsync(new Exception(exceptionMessage));

        var request = new PostConvertFile(contentStream, conversionOptions);

        var handler = new PostConvertFileHandler(_processorMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, exceptionMessage)
            .And.HaveDefaultPayload();
    }
}
