using FluentAssertions;
using Moq;
using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.SubtitleConversion.CommandHandlers;
using SubtitleFileCleanerWeb.Application.SubtitleConversion.Commands;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.SubtitleConversion.CommandHandlers;

public class TestConvertSubtitleFileHandler
{
    private readonly Mock<ISubtitleConversionProcessor> _conversionProcessor;

    public TestConvertSubtitleFileHandler()
    {
        _conversionProcessor = new();
    }

    [Fact]
    public async Task Handle_WithValidParameters_ReturnValid()
    {
        // Arrange
        var contentStream = new MemoryStream(new byte[] { 1, 2 });
        var expectedContentStream = new MemoryStream(new byte[] { 1 }, false);
        var conversionType = (ConversionType)(-1);

        _conversionProcessor.Setup(
            cp => cp.ProcessAsync(
                contentStream,
                conversionType,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OperationResult<Stream> { Payload = expectedContentStream });

        var request = new ConvertSubtitleFile(contentStream, conversionType);
        
        var handler = new ConvertSubtitleFileHandler(_conversionProcessor.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _conversionProcessor.Verify(
            cp => cp.ProcessAsync(
                It.IsAny<Stream>(),
                It.IsAny<ConversionType>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            
            .Which.Should().BeReadOnly()
            .And.HaveLength(expectedContentStream.Length);
    }

    [Fact]
    public async Task ProcessAsync_WithConversionProcessorError_RaiseError()
    {
        // Arrange
        var contentStream = Stream.Null;
        var conversionType = (ConversionType)(-1);

        var conversionResult = new OperationResult<Stream>();

        var errorCode = (ErrorCode)(-1);
        var errorMessage = "Test unexpected error occurred";
        conversionResult.AddError(errorCode, errorMessage);

        _conversionProcessor.Setup(
            cp => cp.ProcessAsync(
                contentStream,
                conversionType,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversionResult);

        var request = new ConvertSubtitleFile(contentStream, conversionType);

        var handler = new ConvertSubtitleFileHandler(_conversionProcessor.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _conversionProcessor.Verify(
            cp => cp.ProcessAsync(
                It.IsAny<Stream>(),
                It.IsAny<ConversionType>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(errorCode, errorMessage)
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task Handle_WithUnexpectedError_ReturnUnknownError()
    {
        // Arrange
        var contentStream = Stream.Null;
        var conversionType = (ConversionType)(-1);

        var exceptionMessage = "Test unexpected error occurred.";
        _conversionProcessor.Setup(
            cp => cp.ProcessAsync(
                contentStream,
                conversionType,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        var request = new ConvertSubtitleFile(contentStream, conversionType);

        var handler = new ConvertSubtitleFileHandler(_conversionProcessor.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _conversionProcessor.Verify(
            cp => cp.ProcessAsync(
                It.IsAny<Stream>(),
                It.IsAny<ConversionType>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, exceptionMessage)
            .And.HaveDefaultPayload();
    }
}
