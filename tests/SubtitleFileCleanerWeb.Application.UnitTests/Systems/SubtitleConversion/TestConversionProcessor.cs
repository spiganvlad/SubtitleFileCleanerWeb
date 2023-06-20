using FluentAssertions;
using Moq;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.SubtitleConversion;
using SubtitleFileCleanerWeb.Application.SubtitleConversion.Abstractions;
using SubtitleFileCleanerWeb.Application.SubtitleConversion.Exceptions;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.Extensions;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.Reflection;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.SubtitleConversion;

public class TestConversionProcessor
{
    private readonly Mock<ISubtitleConverter> _assConverterMock;

    public TestConversionProcessor()
    {
        _assConverterMock = new Mock<ISubtitleConverter>();
        _assConverterMock.Setup(c => c.ConversionType).Returns(ConversionType.Ass);
    }

    [Fact]
    public async Task ProcessAsync_WithValidParameters_ReturnValid()
    {
        // Arrange
        var contentStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }, false);
        var conversionType = ConversionType.Ass;
        var cancellationToken = new CancellationToken();

        var expectedContentStream = new MemoryStream(new byte[] { 2, 4 });
        _assConverterMock.Setup(c => c.ConvertAsync(contentStream, cancellationToken))
            .ReturnsAsync(expectedContentStream);

        var converters = new List<ISubtitleConverter> { _assConverterMock.Object };
        var processor = new SubtitleConversionProcessor(converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, conversionType, cancellationToken);

        // Assert
        _assConverterMock.VerifyGet(c => c.ConversionType, Times.Once());
        _assConverterMock.Verify(c => c.ConvertAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull().And.ContainsNoErrors();
        result.Payload.Should().NotBeNull();
        result.Payload!.Length.Should().Be(expectedContentStream.Length);
    }

    [Fact]
    public async Task ProcessAsync_WithNonExistentConversionType_ReturnUnprocessableContentError()
    {
        // Arrange
        var contentStream = new MemoryStream();
        var conversionType = ConversionType.Srt;
        var cancellationToken = new CancellationToken();

        var converters = new List<ISubtitleConverter> { _assConverterMock.Object };
        var processor = new SubtitleConversionProcessor(converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, conversionType, cancellationToken);

        // Assert
        _assConverterMock.VerifyGet(c => c.ConversionType, Times.Once());
        _assConverterMock.Verify(c => c.ConvertAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Never());

        result.Should().NotBeNull().And
            .ContainSingleError(ErrorCode.UnprocessableContent, $"No converter was found for conversion type: {conversionType}");
        result.Payload.Should().BeNull();
    }

    [Fact]
    public async Task ProcessAsync_WithUnprocessableContent_ReturnUnprocessableContentError()
    {
        // Arrange
        var contentStream = new MemoryStream();
        var cancellationToken = new CancellationToken();

        var exception = InnerExceptionsCreator.Create<NotConvertibleContentException>("Unprocessable content exception");
        _assConverterMock.Setup(c => c.ConvertAsync(contentStream, cancellationToken))
            .ThrowsAsync(exception);

        var converters = new List<ISubtitleConverter> { _assConverterMock.Object };
        var processor = new SubtitleConversionProcessor(converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, ConversionType.Ass, cancellationToken);

        // Assert
        _assConverterMock.Verify(c => c.ConvertAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull().And.ContainSingleError(ErrorCode.UnprocessableContent, exception.Message);
        result.Payload.Should().BeNull();
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

        var converters = new List<ISubtitleConverter> { _assConverterMock.Object };
        var processor = new SubtitleConversionProcessor(converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, ConversionType.Ass, cancellationToken);

        // Assert
        _assConverterMock.VerifyGet(c => c.ConversionType, Times.Once());
        _assConverterMock.Verify(c => c.ConvertAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull().And.ContainSingleError(ErrorCode.UnknownError, exceptionMessage);
        result.Payload.Should().BeNull();
    }
}
