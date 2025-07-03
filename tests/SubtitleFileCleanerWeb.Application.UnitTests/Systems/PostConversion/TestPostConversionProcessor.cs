using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.PostConversion;
using SubtitleFileCleanerWeb.Application.UnitTests.Fixtures.AutoData;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.PostConversion;

public class TestPostConversionProcessor
{
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;
    private readonly List<IPostConverter> _converters;
    private readonly PostConversionProcessor _sut;

    public TestPostConversionProcessor()
    {
        _converters = [];
        _sut = new(_converters);
    }

    [Theory, StreamAutoData]
    public async Task ProcessAsync_WithValidRequest_ReturnStreamResult
        (Stream contentStream, PostConversionOption conversionOption, OperationResult<Stream> converterResult)
    {
        // Arrange
        var converter = Substitute.For<IPostConverter>();

        converter.PostConversionOption.Returns(conversionOption);
        converter
            .ConvertAsync(
                contentStream,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(converterResult));

        _converters.Add(converter);

        // Act
        var result = await _sut.ProcessAsync(contentStream, _cancellationToken, conversionOption);

        // Assert
        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            
            .Which.Should().BeReadOnly()
            .And.HaveLength(converterResult.Payload!.Length);
    }
    
    [Theory, StreamAutoData]
    public async Task ProcessAsync_WithTwoConverters_ReturnStreamResult
        (Stream contentStream, PostConversionOption[] conversionOptions, OperationResult<Stream> firstResult, OperationResult<Stream> secondResult)
    {
        // Arrange
        Array.Resize(ref conversionOptions, conversionOptions.Length -1);

        var firstConverter = Substitute.For<IPostConverter>();

        firstConverter.PostConversionOption.Returns(conversionOptions[0]);
        firstConverter
            .ConvertAsync(
                contentStream,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(firstResult));

        _converters.Add(firstConverter);

        var secondConverter = Substitute.For<IPostConverter>();

        secondConverter.PostConversionOption.Returns(conversionOptions[1]);
        secondConverter
            .ConvertAsync(
                firstResult.Payload!,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(secondResult));

        _converters.Add(secondConverter);

        // Act
        var result = await _sut.ProcessAsync(contentStream, _cancellationToken, conversionOptions);

        // Assert
        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            
            .Which.Should().BeReadOnly()
            .And.HaveLength(secondResult.Payload!.Length);
    }

    [Theory, StreamAutoData]
    public async Task ProcessAsync_WithNonExistentConverter_ReturnPostConversionError
        (Stream contentStream, PostConversionOption conversionOption)
    {
        // Act
        var result = await _sut.ProcessAsync(contentStream, _cancellationToken, conversionOption);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(
                ErrorCode.PostConversionException,
                $"No converter was found for post conversion option: {conversionOption}.")
            .And.HaveDefaultPayload();
    }

    [Theory, StreamAutoData]
    public async Task ProcessAsync_WithPostConverterError_RaiseError
        (Stream contentStream, PostConversionOption conversionOption, ErrorCode code, string message)
    {
        // Arrange
        var converterResult = new OperationResult<Stream>();
        converterResult.AddError(code, message);

        var converter = Substitute.For<IPostConverter>();

        converter.PostConversionOption.Returns(conversionOption);
        converter
            .ConvertAsync(
                contentStream,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(converterResult));

        _converters.Add(converter);

        // Act
        var result = await _sut.ProcessAsync(contentStream, _cancellationToken, conversionOption);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(code, message)
            .And.HaveDefaultPayload();
    }

    [Theory, StreamAutoData]
    public async Task ProcessAsync_WithUnexpectedError_ReturnUnknownError
        (Stream contentStream, PostConversionOption conversionOption, string message)
    {
        // Arrange
        var converter = Substitute.For<IPostConverter>();

        converter.PostConversionOption.Returns(conversionOption);
        converter
            .ConvertAsync(
                Arg.Any<Stream>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(message));

        _converters.Add(converter);

        // Act
        var result = await _sut.ProcessAsync(contentStream, _cancellationToken, conversionOption);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, message)
            .And.HaveDefaultPayload();
    }
}
