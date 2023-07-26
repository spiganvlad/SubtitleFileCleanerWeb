using FluentAssertions;
using Moq;
using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.PostConversion;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.PostConversion;

public class TestPostConversionProcessor
{
    private readonly Mock<IPostConverter> _converter;
    private readonly List<IPostConverter> _converters;

    public TestPostConversionProcessor()
    {
        _converter = new Mock<IPostConverter>();
        _converter.SetupGet(c => c.PostConversionOption)
            .Returns(PostConversionOption.DeleteAssTags);

        _converters = new List<IPostConverter> { _converter.Object };
    }

    [Fact]
    public async Task ProcessAsync_WithValidParameters_ReturnValid()
    {
        // Arrange
        var contentStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, }, false);
        var cancellationToken = new CancellationToken();
        var conversionOption = PostConversionOption.DeleteAssTags;

        var convertedContent = new MemoryStream(new byte[] { 2, 4, 5 }, false);
        var converterResult = new OperationResult<Stream> { Payload = convertedContent };
        _converter.Setup(c => c.ConvertAsync(contentStream, cancellationToken))
            .ReturnsAsync(converterResult);

        var processor = new PostConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, cancellationToken, conversionOption);

        // Assert
        _converter.VerifyGet(c => c.PostConversionOption, Times.Once());
        _converter.Verify(c => c.ConvertAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            
            .Which.Should().BeReadOnly()
            .And.HaveLength(convertedContent.Length);
    }
    
    [Fact]
    public async Task ProcessAsync_WithTwoConverters_ReturnValid()
    {
        // Arrange
        var contentStream = new MemoryStream();
        var cancellationToken = new CancellationToken();
        var conversionOptions = new PostConversionOption[2];
        conversionOptions[0] = PostConversionOption.DeleteAssTags;
        conversionOptions[1] = PostConversionOption.DeleteBasicTags;

        var firstConvertedContent = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }, false);
        var firstConverterResult = new OperationResult<Stream> { Payload = firstConvertedContent };
        _converter.Setup(c => c.ConvertAsync(contentStream, cancellationToken))
            .ReturnsAsync(firstConverterResult);

        var secondConvertedContent = new MemoryStream(new byte[] { 1, 4, 5 }, false);
        var secondConverterResult = new OperationResult<Stream> { Payload = secondConvertedContent };

        var secondConverter = new Mock<IPostConverter>();
        secondConverter.SetupGet(c => c.PostConversionOption)
            .Returns(PostConversionOption.DeleteBasicTags);
        secondConverter.Setup(c => c.ConvertAsync(firstConverterResult.Payload, cancellationToken))
            .ReturnsAsync(secondConverterResult);

        _converters.Add(secondConverter.Object);
        var processor = new PostConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, cancellationToken, conversionOptions);

        // Assert
        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            
            .Which.Should().BeReadOnly()
            .And.HaveLength(secondConvertedContent.Length);
    }

    [Fact]
    public async Task ProcessAsync_WithNonExistentConverter_ReturnPostConversionError()
    {
        // Arrange
        var contentStream = new MemoryStream();
        var cancellationToken = new CancellationToken();
        var conversionOption = PostConversionOption.ToOneLine;

        var processor = new PostConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, cancellationToken, conversionOption);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.PostConversionException,
            $"No converter was found for post conversion option: {conversionOption}")
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task ProcessAsync_WithUnexpectedError_ReturnUnknownError()
    {
        // Arrange
        var contentStream = new MemoryStream();
        var cancellationToken = new CancellationToken();
        var conversionOption = PostConversionOption.DeleteAssTags;

        var exceptionMessage = "Test unexpected error occurred";
        _converter.Setup(c => c.ConvertAsync(contentStream, cancellationToken))
            .ThrowsAsync(new Exception(exceptionMessage));

        var processor = new PostConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, cancellationToken, conversionOption);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, exceptionMessage)
            .And.HaveDefaultPayload();
    }
}
