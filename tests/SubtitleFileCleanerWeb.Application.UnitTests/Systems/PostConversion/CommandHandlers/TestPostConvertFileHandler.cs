using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.PostConversion.CommandHandlers;
using SubtitleFileCleanerWeb.Application.PostConversion.Commands;
using SubtitleFileCleanerWeb.Application.UnitTests.Fixtures.AutoData;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.PostConversion.CommandHandlers;

public class TestPostConvertFileHandler
{
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;
    private readonly IPostConversionProcessor _processorMock;
    private readonly PostConvertFileHandler _sut;

    public TestPostConvertFileHandler()
    {
        _processorMock = Substitute.For<IPostConversionProcessor>();
        _sut = new(_processorMock);
    }

    [Theory, StreamAutoData]
    public async Task Handle_WithValidRequest_ReturnStreamResult
        (PostConvertFile request, OperationResult<Stream> processorResult)
    {
        // Arrange
        _processorMock
            .ProcessAsync(
                request.ContentStream,
                Arg.Any<CancellationToken>(),
                request.ConversionOptions)
            .Returns(Task.FromResult(processorResult));

        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            
            .Which.Should().BeReadOnly()
            .And.HaveLength(processorResult.Payload!.Length);
    }

    [Theory, StreamAutoData]
    public async Task Handle_WithProcessorError_RaiseError
        (PostConvertFile request, ErrorCode code, string message)
    {
        // Arrange
        var processorResult = new OperationResult<Stream>();
        processorResult.AddError(code, message);

        _processorMock
            .ProcessAsync(
                request.ContentStream,
                Arg.Any<CancellationToken>(),
                request.ConversionOptions)
            .Returns(Task.FromResult(processorResult));

        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(code, message)
            .And.HaveDefaultPayload();
    }

    [Theory, StreamAutoData]
    public async Task Handle_WithUnexpectedException_ReturnUnknowError
        (PostConvertFile request, string message)
    {
        // Arrange
        _processorMock
            .ProcessAsync(
                Arg.Any<Stream>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<PostConversionOption[]>())
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
