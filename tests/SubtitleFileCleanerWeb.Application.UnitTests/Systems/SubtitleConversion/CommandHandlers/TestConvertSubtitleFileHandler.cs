using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.SubtitleConversion.CommandHandlers;
using SubtitleFileCleanerWeb.Application.SubtitleConversion.Commands;
using SubtitleFileCleanerWeb.Application.UnitTests.Fixtures.AutoData;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.SubtitleConversion.CommandHandlers;

public class TestConvertSubtitleFileHandler
{
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;
    private readonly ISubtitleConversionProcessor _conversionProcessor;
    private readonly ConvertSubtitleFileHandler _sut;

    public TestConvertSubtitleFileHandler()
    {
        _conversionProcessor = Substitute.For<ISubtitleConversionProcessor>();
        _sut = new(_conversionProcessor);
    }

    [Theory, StreamAutoData]
    public async Task Handle_WithValidRequest_ReturnStreamResult
        (ConvertSubtitleFile request, OperationResult<Stream> conversionResult)
    {
        // Arrange
        _conversionProcessor
            .ProcessAsync(
                request.ContentStream,
                request.ConversionType,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(conversionResult));

        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            
            .Which.Should().BeReadOnly()
            .And.HaveLength(conversionResult.Payload!.Length);
    }

    [Theory, StreamAutoData]
    public async Task ProcessAsync_WithConversionProcessorError_RaiseError
        (ConvertSubtitleFile request, ErrorCode code, string message)
    {
        // Arrange
        var conversionResult = new OperationResult<Stream>();
        conversionResult.AddError(code, message);

        _conversionProcessor
            .ProcessAsync(
                request.ContentStream,
                request.ConversionType,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(conversionResult));

        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(code, message)
            .And.HaveDefaultPayload();
    }

    [Theory, StreamAutoData]
    public async Task Handle_WithUnexpectedError_ReturnUnknownError
        (ConvertSubtitleFile request, string message)
    {
        // Arrange
        _conversionProcessor
            .ProcessAsync(
                Arg.Any<Stream>(),
                Arg.Any<ConversionType>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(message));

        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, message)
            .And.HaveDefaultPayload();
    }
}
