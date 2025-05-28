using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.PostConversion;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.PostConversion;

public class TestPostConversionProcessor
{
    private readonly Mock<IPostConverter> _minusOneConverter;
    private readonly List<IPostConverter> _converters;

    public TestPostConversionProcessor()
    {
        _minusOneConverter = new();
        _minusOneConverter.SetupGet(c => c.PostConversionOption)
            .Returns((PostConversionOption)(-1));

        _converters = new List<IPostConverter>
        {
            _minusOneConverter.Object
        };
    }

    [Fact]
    public async Task ProcessAsync_WithValidParameters_ReturnValid()
    {
        // Arrange
        var contentStream = new MemoryStream([1, 2], false);
        var conversionOption = (PostConversionOption)(-1);

        var convertedContent = new MemoryStream([1], false);
        var converterResult = new OperationResult<Stream>
        {
            Payload = convertedContent
        };

        _minusOneConverter.Setup(
            c => c.ConvertAsync(
                contentStream,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(converterResult);

        var processor = new PostConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, default, conversionOption);

        // Assert
        _minusOneConverter.VerifyGet(
            c => c.PostConversionOption,
            Times.Once());

        _minusOneConverter.Verify(
            c => c.ConvertAsync(
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

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
        var contentStream = Stream.Null;
        PostConversionOption[] conversionOptions =
        [
            (PostConversionOption)(-1),
            (PostConversionOption)(-2)
        ];

        var assConvertedContent = new MemoryStream([1, 2], false);
        var assConverterResult = new OperationResult<Stream>
        {
            Payload = assConvertedContent
        };

        _minusOneConverter.Setup(
            c => c.ConvertAsync(
                contentStream,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(assConverterResult);

        var basicConvertedContent = new MemoryStream([1], false);
        var basicConverterResult = new OperationResult<Stream>
        {
            Payload = basicConvertedContent
        };

        var basicTagsConverter = new Mock<IPostConverter>();
        basicTagsConverter.SetupGet(c => c.PostConversionOption)
            .Returns((PostConversionOption)(-2));

        basicTagsConverter.Setup(
            c => c.ConvertAsync(
                assConverterResult.Payload,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(basicConverterResult);

        _converters.Add(basicTagsConverter.Object);
        var processor = new PostConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, default, conversionOptions);

        // Assert
        _minusOneConverter.VerifyGet(
            c => c.PostConversionOption,
            Times.Exactly(2));

        _minusOneConverter.Verify(
            c => c.ConvertAsync(
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        basicTagsConverter.VerifyGet(
            c => c.PostConversionOption,
            Times.Once());

        basicTagsConverter.Verify(
            c => c.ConvertAsync(
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

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
        var contentStream = Stream.Null;
        var conversionOption = (PostConversionOption)(-2);

        var processor = new PostConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, default, conversionOption);

        // Assert
        _minusOneConverter.VerifyGet(
            c => c.PostConversionOption,
            Times.Once());

        _minusOneConverter.Verify(
            c => c.ConvertAsync(
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()),
            Times.Never());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(
                ErrorCode.PostConversionException,
                $"No converter was found for post conversion option: {conversionOption}.")
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task ProcessAsync_WithPostConverterError_RaiseError()
    {
        // Arrange
        var contentStream = Stream.Null;
        var conversionOption = (PostConversionOption)(-1);

        var converterResult = new OperationResult<Stream>();

        var errorCode = (ErrorCode)(-1);
        var errorMessage = "Test unexpected error occurred.";
        converterResult.AddError(errorCode, errorMessage);

        _minusOneConverter.Setup(
            c => c.ConvertAsync(
                contentStream,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(converterResult);

        var processor = new PostConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, default, conversionOption);

        // Assert
        _minusOneConverter.VerifyGet(
            c => c.PostConversionOption,
            Times.Once());

        _minusOneConverter.Verify(
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
    public async Task ProcessAsync_WithUnexpectedError_ReturnUnknownError()
    {
        // Arrange
        var contentStream = Stream.Null;
        var conversionOption = (PostConversionOption)(-1);

        var exceptionMessage = "Test unexpected error occurred.";
        _minusOneConverter.Setup(
            c => c.ConvertAsync(
                contentStream,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        var processor = new PostConversionProcessor(_converters);

        // Act
        var result = await processor.ProcessAsync(contentStream, default, conversionOption);

        // Assert
        _minusOneConverter.VerifyGet(
            c => c.PostConversionOption,
            Times.Once());

        _minusOneConverter.Verify(
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
