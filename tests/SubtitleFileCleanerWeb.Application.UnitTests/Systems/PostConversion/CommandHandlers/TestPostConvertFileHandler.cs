using FluentAssertions;
using Moq;
using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.PostConversion.CommandHandlers;
using SubtitleFileCleanerWeb.Application.PostConversion.Commands;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.PostConversion.CommandHandlers;

public class TestPostConvertFileHandler
{
    private readonly Mock<IPostConversionProcessor> _processorMock;

    public TestPostConvertFileHandler()
    {
        _processorMock = new Mock<IPostConversionProcessor>();
    }

    [Fact]
    public async Task Handle_WithValidParameters_ReturnValid()
    {
        // Arrange
        var contentStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
        var expectedContentStream = new MemoryStream(new byte[] { 1, 3, 5 }, false);
        var conversionOptions = new PostConversionOption[2];
        conversionOptions[0] = PostConversionOption.DeleteAssTags;
        conversionOptions[1] = PostConversionOption.ToOneLine;
        var cancellationToken = new CancellationToken();

        var processorResult = new OperationResult<Stream> { Payload = expectedContentStream };
        _processorMock.Setup(p => p.ProcessAsync(contentStream, cancellationToken, conversionOptions))
            .ReturnsAsync(processorResult);

        var request = new PostConvertFile(contentStream, conversionOptions);

        var handler = new PostConvertFileHandler(_processorMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _processorMock.Verify(p => p.ProcessAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>(),
            It.IsAny<PostConversionOption[]>()), Times.Once());

        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            
            .Which.Should().BeReadOnly()
            .And.HaveLength(expectedContentStream.Length);
    }

    [Fact]
    public async Task Handle_WithProcessorError_RaiseError()
    {
        // Arrange
        var contentStream = new MemoryStream();
        var conversionOptions = Array.Empty<PostConversionOption>();
        var cancellationToken = new CancellationToken();

        string errorMessage = "Test unexpected error occurred";
        var processorResult = new OperationResult<Stream>();
        processorResult.AddError(ErrorCode.UnknownError, errorMessage);
        _processorMock.Setup(p => p.ProcessAsync(contentStream, cancellationToken, conversionOptions))
            .ReturnsAsync(processorResult);

        var request = new PostConvertFile(contentStream, conversionOptions);

        var handler = new PostConvertFileHandler(_processorMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _processorMock.Verify(p => p.ProcessAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>(),
            It.IsAny<PostConversionOption[]>()), Times.Once());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, errorMessage)
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task Handle_WithUnexpectedException_ReturnUnknowError()
    {
        // Arrange
        var contentStream = new MemoryStream();
        var conversionOptions = Array.Empty<PostConversionOption>();
        var cancellationToken = new CancellationToken();

        var exceptionMessage = "Test unexpected exception occurred";
        _processorMock.Setup(p => p.ProcessAsync(contentStream, cancellationToken, conversionOptions))
            .ThrowsAsync(new Exception(exceptionMessage));

        var request = new PostConvertFile(contentStream, conversionOptions);

        var handler = new PostConvertFileHandler(_processorMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, exceptionMessage)
            .And.HaveDefaultPayload();
    }
}
