using FluentAssertions;
using Moq;
using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Exceptions;
using SubtitleFileCleanerWeb.Application.PostConversion;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.Extensions;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.Reflection;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.PostConversion;

public class TestPostConversionProcessor
{
    private readonly Mock<IPostConverter> _converter;
    private readonly List<IPostConverter> _converters;

    public TestPostConversionProcessor()
    {
        _converter = new Mock<IPostConverter>();
        _converter.SetupGet(c => c.PostConversionOption)
            .Returns(PostConversionOption.DeleteTags);

        _converters = new List<IPostConverter> { _converter.Object };
    }

    [Fact]
    public async Task ProcessAsync_WithValidParameters_ReturnValid()
    {
        // Arrange
        var contentStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, });
        var conversionType = ConversionType.Ass;
        var cancellationToken = new CancellationToken();
        var conversionOption = PostConversionOption.DeleteTags;

        var converterResult = new MemoryStream(new byte[] { 2, 4, 5 });
        _converter.Setup(c => c.ConvertAsync(contentStream, conversionType, cancellationToken))
            .ReturnsAsync(converterResult);

        var processor = new PostConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, conversionType, cancellationToken, conversionOption);

        // Assert
        _converter.VerifyGet(c => c.PostConversionOption, Times.Once());
        _converter.Verify(c => c.ConvertAsync(It.IsAny<Stream>(), It.IsAny<ConversionType>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull().And.ContainsNoErrors();
        result.Payload.Should().NotBeNull()
            .And.BeReadable()
            .And.HaveLength(converterResult.Length);
    }

    [Fact]
    public async Task ProcessAsync_WithTwoConverters_ReturnValid()
    {
        // Arrange
        var contentStream = new MemoryStream();
        var conversionType = ConversionType.Ass;
        var cancellationToken = new CancellationToken();
        var conversionOptions = new PostConversionOption[2];
        conversionOptions[0] = PostConversionOption.DeleteTags;
        conversionOptions[1] = PostConversionOption.ToOneLine;

        var firstConverterResult = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
        _converter.Setup(c => c.ConvertAsync(contentStream, conversionType, cancellationToken))
            .ReturnsAsync(firstConverterResult);

        var secondConverterResult = new MemoryStream(new byte[] { 1, 4, 5 });
        var secondConverter = new Mock<IPostConverter>();
        secondConverter.SetupGet(c => c.PostConversionOption)
            .Returns(PostConversionOption.ToOneLine);
        secondConverter.Setup(c => c.ConvertAsync(firstConverterResult, conversionType, cancellationToken))
            .ReturnsAsync(secondConverterResult);

        _converters.Add(secondConverter.Object);
        var processor = new PostConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, conversionType, cancellationToken, conversionOptions);

        // Assert
        result.Should().NotBeNull().And.ContainsNoErrors();
        result.Payload.Should().NotBeNull()
            .And.BeReadable()
            .And.HaveLength(secondConverterResult.Length);
    }

    [Fact]
    public async Task ProcessAsync_WithNonExistentConverter_ReturnPostConversionError()
    {
        // Arrange
        var contentStream = new MemoryStream();
        var conversionType = ConversionType.Ass;
        var cancellationToken = new CancellationToken();
        var conversionOption = PostConversionOption.ToOneLine;

        var processor = new PostConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, conversionType, cancellationToken, conversionOption);

        // Assert
        result.Should().NotBeNull().And.ContainSingleError(ErrorCode.PostConversionException,
            $"No converter was found for post conversion option: {conversionOption}");
        result.Payload.Should().BeNull();
    }

    [Fact]
    public async Task ProcessAsync_WithNotConvertibleContent_ReturnUnprocessableContentError()
    {
        // Arrange
        var contentStream = new MemoryStream();
        var conversionType = ConversionType.Ass;
        var cancellationToken = new CancellationToken();
        var conversionOption = PostConversionOption.DeleteTags;

        var exceptionMessage = "Test content is unprocessable";
        var exception = InnerExceptionsCreator.Create<NotConvertibleContentException>(exceptionMessage);
        _converter.Setup(c => c.ConvertAsync(contentStream, conversionType, cancellationToken))
            .ThrowsAsync(exception);

        var processor = new PostConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, conversionType, cancellationToken, conversionOption);

        // Assert
        result.Should().NotBeNull().And.ContainSingleError(ErrorCode.UnprocessableContent, exceptionMessage);
        result.Payload.Should().BeNull();
    }

    [Fact]
    public async Task ProcessAsync_WithUnexpectedError_ReturnUnknownError()
    {
        // Arrange
        var contentStream = new MemoryStream();
        var conversionType = ConversionType.Ass;
        var cancellationToken = new CancellationToken();
        var conversionOption = PostConversionOption.DeleteTags;

        var exceptionMessage = "Test unexpected error occurred";
        _converter.Setup(c => c.ConvertAsync(contentStream, conversionType, cancellationToken))
            .ThrowsAsync(new Exception(exceptionMessage));

        var processor = new PostConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, conversionType, cancellationToken, conversionOption);

        // Assert
        result.Should().NotBeNull().And.ContainSingleError(ErrorCode.UnknownError, exceptionMessage);
        result.Payload.Should().BeNull();
    }
}
