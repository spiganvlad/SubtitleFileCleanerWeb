using FluentAssertions;
using Moq;
using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.SubtitleConversion;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.SubtitleConversion;

public class TestSubtitleConversionProcessor
{
    private readonly Mock<ISubtitleConverter> _assConverterMock;
    private readonly List<ISubtitleConverter> _converters;

    public TestSubtitleConversionProcessor()
    {
        _assConverterMock = new Mock<ISubtitleConverter>();
        _assConverterMock.Setup(c => c.ConversionType).Returns(ConversionType.Ass);

        _converters = new List<ISubtitleConverter> { _assConverterMock.Object };
    }

    [Fact]
    public async Task ProcessAsync_WithValidParameters_ReturnValid()
    {
        // Arrange
        var contentStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }, false);
        var conversionType = ConversionType.Ass;
        var cancellationToken = new CancellationToken();

        var expectedContentStream = new MemoryStream(new byte[] { 2, 4 }, false);
        var converterResult = new OperationResult<Stream> { Payload = expectedContentStream };
        _assConverterMock.Setup(c => c.ConvertAsync(contentStream, cancellationToken))
            .ReturnsAsync(converterResult);

        var processor = new SubtitleConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, conversionType, cancellationToken);

        // Assert
        _assConverterMock.VerifyGet(c => c.ConversionType, Times.Once());
        _assConverterMock.Verify(c => c.ConvertAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            
            .Which.Should().BeReadOnly()
            .And.HaveLength(expectedContentStream.Length);
    }

    [Fact]
    public async Task ProcessAsync_WithNonExistentConversionType_ReturnSubtitleConversionError()
    {
        // Arrange
        var contentStream = new MemoryStream();
        var conversionType = ConversionType.Srt;
        var cancellationToken = new CancellationToken();

        var processor = new SubtitleConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, conversionType, cancellationToken);

        // Assert
        _assConverterMock.VerifyGet(c => c.ConversionType, Times.Once());
        _assConverterMock.Verify(c => c.ConvertAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Never());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.SubtitleConversionException, $"No converter was found for conversion type: {conversionType}")
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task ProcessAsync_WithConverterError_RaiseError()
    {
        // Arrange
        var contentStream = new MemoryStream();
        var converterType = ConversionType.Ass;
        var cancellationToken = new CancellationToken();

        var errorMessage = "Test unexpected error occurred.";
        var errorCode = ErrorCode.UnknownError;

        var converterResult = new OperationResult<Stream>();
        converterResult.AddError(errorCode, errorMessage);
        _assConverterMock.Setup(c => c.ConvertAsync(contentStream, cancellationToken))
            .ReturnsAsync(converterResult);

        var processor = new SubtitleConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, converterType, cancellationToken);

        // Assert
        _assConverterMock.VerifyGet(c => c.ConversionType, Times.Once());
        _assConverterMock.Verify(c => c.ConvertAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(errorCode, errorMessage)
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task ProcessAsync_WithUnknownException_ReturnUnknownError()
    {
        // Arrange
        var contentStream = new MemoryStream();
        var cancellationToken = new CancellationToken();

        var exceptionMessage = "Unexpected error occurred";
        _assConverterMock.Setup(c => c.ConvertAsync(contentStream, cancellationToken))
            .ThrowsAsync(new Exception(exceptionMessage));

        var processor = new SubtitleConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, ConversionType.Ass, cancellationToken);

        // Assert
        _assConverterMock.VerifyGet(c => c.ConversionType, Times.Once());
        _assConverterMock.Verify(c => c.ConvertAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, exceptionMessage)
            .And.HaveDefaultPayload();
    }
}
