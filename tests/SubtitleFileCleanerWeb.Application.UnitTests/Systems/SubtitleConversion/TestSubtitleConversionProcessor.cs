using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.SubtitleConversion;
using SubtitleFileCleanerWeb.Application.UnitTests.Fixtures.AutoData;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.SubtitleConversion;

public class TestSubtitleConversionProcessor
{
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;
    private readonly List<ISubtitleConverter> _converters;
    private readonly SubtitleConversionProcessor _sut;

    public TestSubtitleConversionProcessor()
    {
        _converters = [];
        _sut = new(_converters);
    }

    [Theory, StreamAutoData]
    public async Task ProcessAsync_WithValidParameters_ReturnStreamResult
        (Stream contentStream, ConversionType conversionType, OperationResult<Stream> converterResult)
    {
        // Arrange
        var converter = Substitute.For<ISubtitleConverter>();
        
        converter.ConversionType.Returns(conversionType);
        converter
            .ConvertAsync(
                contentStream,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(converterResult));

        _converters.Add(converter);

        // Act
        var result = await _sut.ProcessAsync(contentStream, conversionType, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            
            .Which.Should().BeReadOnly()
            .And.HaveLength(converterResult.Payload!.Length);
    }

    [Theory, StreamAutoData]
    public async Task ProcessAsync_WithNonExistentConversionType_ReturnSubtitleConversionError
        (Stream contentStream, ConversionType conversionType)
    {
        // Act
        var result = await _sut.ProcessAsync(contentStream, conversionType, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(
                ErrorCode.SubtitleConversionException,
                $"No converter was found for conversion type: {conversionType}.")
            .And.HaveDefaultPayload();
    }

    [Theory, StreamAutoData]
    public async Task ProcessAsync_WithConverterError_RaiseError
        (Stream contentStream, ConversionType conversionType, ErrorCode code, string message)
    {
        // Arrange
        var converterResult = new OperationResult<Stream>();
        converterResult.AddError(code, message);

        var converter = Substitute.For<ISubtitleConverter>();

        converter.ConversionType.Returns(conversionType);
        converter
            .ConvertAsync(
                contentStream,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(converterResult));

        _converters.Add(converter);

        // Act
        var result = await _sut.ProcessAsync(contentStream, conversionType, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(code, message)
            .And.HaveDefaultPayload();
    }

    [Theory, StreamAutoData]
    public async Task ProcessAsync_WithUnknownException_ReturnUnknownError
        (Stream contentStream, ConversionType conversionType, string message)
    {
        // Arrange
        var converter = Substitute.For<ISubtitleConverter>();

        converter.ConversionType.Returns(conversionType);
        converter
            .ConvertAsync(
                Arg.Any<Stream>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(message));

        _converters.Add(converter);

        // Act
        var result = await _sut.ProcessAsync(contentStream, conversionType, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, message)
            .And.HaveDefaultPayload();
    }
}
