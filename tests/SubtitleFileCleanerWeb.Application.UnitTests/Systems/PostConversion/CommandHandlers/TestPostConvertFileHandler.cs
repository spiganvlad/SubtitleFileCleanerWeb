using FluentAssertions;
using Moq;
using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.PostConversion.CommandHandlers;
using SubtitleFileCleanerWeb.Application.PostConversion.Commands;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.Extensions;

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
        var conversionType = ConversionType.Ass;
        var conversionOptions = new PostConversionOption[2];
        conversionOptions[0] = PostConversionOption.DeleteTags;
        conversionOptions[1] = PostConversionOption.ToOneLine;
        var cancellationToken = new CancellationToken();

        var processorResult = new OperationResult<Stream> { Payload = expectedContentStream };
        _processorMock.Setup(p => p.ProcessAsync(contentStream, conversionType, cancellationToken, conversionOptions))
            .ReturnsAsync(processorResult);

        var request = new PostConvertFile(contentStream, conversionType, conversionOptions);

        var handler = new PostConvertFileHandler(_processorMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _processorMock.Verify(p => p.ProcessAsync(It.IsAny<Stream>(), It.IsAny<ConversionType>(),
            It.IsAny<CancellationToken>(), It.IsAny<PostConversionOption[]>()), Times.Once());

        result.Should().NotBeNull().And.ContainsNoErrors();
        result.Payload.Should().NotBeNull()
            .And.HaveLength(expectedContentStream.Length)
            .And.BeReadOnly();
    }

    [Fact]
    public async Task Handle_WithProcessorError_RaiseError()
    {
        // Arrange
        var contentStream = new MemoryStream();
        var conversionType = ConversionType.Ass;
        var conversionOptions = Array.Empty<PostConversionOption>();
        var cancellationToken = new CancellationToken();

        string errorMessage = "Test unexpected error occurred";
        var processorResult = new OperationResult<Stream>();
        processorResult.AddError(ErrorCode.UnknownError, errorMessage);
        _processorMock.Setup(p => p.ProcessAsync(contentStream, conversionType, cancellationToken, conversionOptions))
            .ReturnsAsync(processorResult);

        var request = new PostConvertFile(contentStream, conversionType, conversionOptions);

        var handler = new PostConvertFileHandler(_processorMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _processorMock.Verify(p => p.ProcessAsync(It.IsAny<Stream>(), It.IsAny<ConversionType>(),
            It.IsAny<CancellationToken>(), It.IsAny<PostConversionOption[]>()), Times.Once());

        result.Should().NotBeNull().And.ContainSingleError(ErrorCode.UnknownError, errorMessage);
        result.Payload.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithUnexpectedException_ReturnUnknowError()
    {
        // Arrange
        var contentStream = new MemoryStream();
        var conversionType = ConversionType.Ass;
        var conversionOptions = Array.Empty<PostConversionOption>();
        var cancellationToken = new CancellationToken();

        var exceptionMessage = "Test unexpected exception occurred";
        _processorMock.Setup(p => p.ProcessAsync(contentStream, conversionType, cancellationToken, conversionOptions))
            .ThrowsAsync(new Exception(exceptionMessage));

        var request = new PostConvertFile(contentStream, conversionType, conversionOptions);

        var handler = new PostConvertFileHandler(_processorMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        result.Should().NotBeNull().And.ContainSingleError(ErrorCode.UnknownError, exceptionMessage);
        result.Payload.Should().BeNull();
    }
}
