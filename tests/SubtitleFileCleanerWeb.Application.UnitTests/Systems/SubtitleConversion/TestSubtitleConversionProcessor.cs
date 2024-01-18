using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.SubtitleConversion;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.SubtitleConversion;

public class TestSubtitleConversionProcessor
{
    private readonly Mock<ISubtitleConverter> _minusOneConverterMock;
    private readonly List<ISubtitleConverter> _converters;

    public TestSubtitleConversionProcessor()
    {
        _minusOneConverterMock = new();
        _minusOneConverterMock.SetupGet(c => c.ConversionType)
            .Returns((ConversionType)(-1));

        _converters = new List<ISubtitleConverter>
        {
            _minusOneConverterMock.Object
        };
    }

    [Fact]
    public async Task ProcessAsync_WithValidParameters_ReturnValid()
    {
        // Arrange
        var contentStream = new MemoryStream(new byte[] { 1, 2 }, false);
        var conversionType = (ConversionType)(-1);

        var expectedContentStream = new MemoryStream(new byte[] { 1 }, false);
        var converterResult = new OperationResult<Stream>
        {
            Payload = expectedContentStream
        };

        _minusOneConverterMock.Setup(
            c => c.ConvertAsync(
                contentStream,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(converterResult);

        var processor = new SubtitleConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, conversionType, default);

        // Assert
        _minusOneConverterMock.VerifyGet(
            c => c.ConversionType,
            Times.Once());

        _minusOneConverterMock.Verify(
            c => c.ConvertAsync(
                It.IsAny<Stream>(),
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
    public async Task ProcessAsync_WithNonExistentConversionType_ReturnSubtitleConversionError()
    {
        // Arrange
        var contentStream = Stream.Null;
        var conversionType = (ConversionType)(-2);

        var processor = new SubtitleConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, conversionType, default);

        // Assert
        _minusOneConverterMock.VerifyGet(
            c => c.ConversionType,
            Times.Once());

        _minusOneConverterMock.Verify(
            c => c.ConvertAsync(
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()),
            Times.Never());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.SubtitleConversionException, $"No converter was found for conversion type: {conversionType}.")
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task ProcessAsync_WithConverterError_RaiseError()
    {
        // Arrange
        var contentStream = Stream.Null;
        var converterType = (ConversionType)(-1);

        var converterResult = new OperationResult<Stream>();

        var errorCode = (ErrorCode)(-1);
        var errorMessage = "Test unexpected error occurred.";
        converterResult.AddError(errorCode, errorMessage);

        _minusOneConverterMock.Setup(
            c => c.ConvertAsync(
                contentStream,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(converterResult);

        var processor = new SubtitleConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, converterType, default);

        // Assert
        _minusOneConverterMock.VerifyGet(
            c => c.ConversionType,
            Times.Once());

        _minusOneConverterMock.Verify(
            c => c.ConvertAsync(
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(errorCode, errorMessage)
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task ProcessAsync_WithUnknownException_ReturnUnknownError()
    {
        // Arrange
        var contentStream = Stream.Null;
        var converterType = (ConversionType)(-1);

        var exceptionMessage = "Test unexpected error occurred.";
        _minusOneConverterMock.Setup(
            c => c.ConvertAsync(
                contentStream,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        var processor = new SubtitleConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, converterType, default);

        // Assert
        _minusOneConverterMock.VerifyGet(
            c => c.ConversionType,
            Times.Once());

        _minusOneConverterMock.Verify(
            c => c.ConvertAsync(
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, exceptionMessage)
            .And.HaveDefaultPayload();
    }
}
