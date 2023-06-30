using FluentAssertions;
using Moq;
using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.SubtitleConversion.CommandHandlers;
using SubtitleFileCleanerWeb.Application.SubtitleConversion.Commands;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.Extensions;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.SubtitleConversion.CommandHandlers;

public class TestConvertSubtitleFileHandler
{
    private readonly Mock<ISubtitleConversionProcessor> _conversionProcessor;

    public TestConvertSubtitleFileHandler()
    {
        _conversionProcessor = new Mock<ISubtitleConversionProcessor>();
    }

    [Fact]
    public async Task Handle_WithValidParameters_ReturnValid()
    {
        // Arrange
        var contentStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
        var expectedContentStream = new MemoryStream(new byte[] { 1, 4, 5 }, false);
        var conversionType = ConversionType.Ass;
        var cancellationToken = new CancellationToken();

        var conversionResult = new OperationResult<Stream> { Payload = expectedContentStream };
        _conversionProcessor.Setup(cp => cp.ProcessAsync(contentStream, conversionType, cancellationToken))
            .ReturnsAsync(conversionResult);

        var request = new ConvertSubtitleFile(contentStream, conversionType);
        
        var handler = new ConvertSubtitleFileHandler(_conversionProcessor.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _conversionProcessor.Verify(cp => cp.ProcessAsync(It.IsAny<Stream>(), It.IsAny<ConversionType>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull().And.ContainsNoErrors();
        result.Payload.Should().NotBeNull()
            .And.HaveLength(expectedContentStream.Length)
            .And.BeReadOnly();
    }

    [Fact]
    public async Task Handle_WithConversionProcessorError_ReturnError()
    {
        // Arrange
        var contentStream = new MemoryStream();
        var conversionType = ConversionType.Ass;
        var cancellationToken = new CancellationToken();

        var errorMessage = "Test unexpected error occurred";
        var conversionResult = new OperationResult<Stream>();
        conversionResult.AddUnknownError(errorMessage);
        _conversionProcessor.Setup(cp => cp.ProcessAsync(contentStream, conversionType, cancellationToken))
            .ReturnsAsync(conversionResult);

        var request = new ConvertSubtitleFile(contentStream, conversionType);

        var handler = new ConvertSubtitleFileHandler(_conversionProcessor.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _conversionProcessor.Verify(cp => cp.ProcessAsync(It.IsAny<Stream>(), It.IsAny<ConversionType>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull().And.ContainSingleError(ErrorCode.UnknownError, errorMessage);
        result.Payload.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithUnexpectedError_ReturnUnknownError()
    {
        // Arrange
        var contentStream = new MemoryStream();
        var conversionType = ConversionType.Ass;
        var cancellationToken = new CancellationToken();

        var exceptionMessage = "Test unexpected error occurred";
        _conversionProcessor.Setup(cp => cp.ProcessAsync(contentStream, conversionType, cancellationToken))
            .ThrowsAsync(new Exception(exceptionMessage));

        var request = new ConvertSubtitleFile(contentStream, conversionType);

        var handler = new ConvertSubtitleFileHandler(_conversionProcessor.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _conversionProcessor.Verify(cp => cp.ProcessAsync(It.IsAny<Stream>(), It.IsAny<ConversionType>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull().And.ContainSingleError(ErrorCode.UnknownError, exceptionMessage);
        result.Payload.Should().BeNull();
    }
}
