using FluentAssertions;
using Moq;
using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.PostConversion;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.PostConversion;

public class TestPostConversionProcessor
{
    private readonly Mock<IPostConverter> _assTagsConverter;
    private readonly List<IPostConverter> _converters;

    public TestPostConversionProcessor()
    {
        _assTagsConverter = new Mock<IPostConverter>();
        _assTagsConverter.SetupGet(c => c.PostConversionOption)
            .Returns(PostConversionOption.DeleteAssTags);

        _converters = new List<IPostConverter> { _assTagsConverter.Object };
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
        _assTagsConverter.Setup(c => c.ConvertAsync(contentStream, cancellationToken))
            .ReturnsAsync(converterResult);

        var processor = new PostConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, cancellationToken, conversionOption);

        // Assert
        _assTagsConverter.VerifyGet(c => c.PostConversionOption, Times.Once());
        _assTagsConverter.Verify(c => c.ConvertAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once());

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

        var assConvertedContent = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }, false);
        var assConverterResult = new OperationResult<Stream> { Payload = assConvertedContent };
        _assTagsConverter.Setup(c => c.ConvertAsync(contentStream, cancellationToken))
            .ReturnsAsync(assConverterResult);

        var basicConvertedContent = new MemoryStream(new byte[] { 1, 4, 5 }, false);
        var basicConverterResult = new OperationResult<Stream> { Payload = basicConvertedContent };

        var basicTagsConverter = new Mock<IPostConverter>();
        basicTagsConverter.SetupGet(c => c.PostConversionOption)
            .Returns(PostConversionOption.DeleteBasicTags);
        basicTagsConverter.Setup(c => c.ConvertAsync(assConverterResult.Payload, cancellationToken))
            .ReturnsAsync(basicConverterResult);

        _converters.Add(basicTagsConverter.Object);
        var processor = new PostConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, cancellationToken, conversionOptions);

        // Assert
        _assTagsConverter.VerifyGet(c => c.PostConversionOption, Times.Exactly(2));
        _assTagsConverter.Verify(c => c.ConvertAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once());
        basicTagsConverter.VerifyGet(c => c.PostConversionOption, Times.Once());
        basicTagsConverter.Verify(c => c.ConvertAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            
            .Which.Should().BeReadOnly()
            .And.HaveLength(basicConvertedContent.Length);
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
        _assTagsConverter.VerifyGet(c => c.PostConversionOption, Times.Once());
        _assTagsConverter.Verify(c => c.ConvertAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Never());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.PostConversionException,
            $"No converter was found for post conversion option: {conversionOption}")
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task ProcessAsync_WithPostConverterError_RaiseError()
    {
        // Arrange
        var contentStream = new MemoryStream();
        var cancellationToken = new CancellationToken();
        var conversionOption = PostConversionOption.DeleteAssTags;

        var errorMessage = "Test unexpected error occurred.";
        var errorCode = ErrorCode.UnknownError;

        var converterResult = new OperationResult<Stream>();
        converterResult.AddError(errorCode, errorMessage);
        _assTagsConverter.Setup(c => c.ConvertAsync(contentStream, cancellationToken))
            .ReturnsAsync(converterResult);

        var processor = new PostConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, cancellationToken, conversionOption);

        // Assert
        _assTagsConverter.VerifyGet(c => c.PostConversionOption, Times.Once());
        _assTagsConverter.Verify(c => c.ConvertAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(errorCode, errorMessage)
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
        _assTagsConverter.Setup(c => c.ConvertAsync(contentStream, cancellationToken))
            .ThrowsAsync(new Exception(exceptionMessage));

        var processor = new PostConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, cancellationToken, conversionOption);

        // Assert
        _assTagsConverter.VerifyGet(c => c.PostConversionOption, Times.Once());
        _assTagsConverter.Verify(c => c.ConvertAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, exceptionMessage)
            .And.HaveDefaultPayload();
    }
}
