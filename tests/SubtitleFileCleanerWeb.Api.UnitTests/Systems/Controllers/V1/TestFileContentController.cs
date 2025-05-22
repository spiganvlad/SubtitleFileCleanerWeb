using SubtitleFileCleanerWeb.Api.Contracts.Common;
using SubtitleFileCleanerWeb.Api.Controllers.V1;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Builders;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContexts.Queries;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Systems.Controllers.V1;

public class TestFileContentController
{
    private readonly FileContentController _controller;
    private readonly Mock<IMediator> _mediatorMock;

    public TestFileContentController()
    {
        _mediatorMock = new();

        var httpContext = new HttpContextMockObjectBuilder()
            .SetupIMediator(_mediatorMock)
            .Build();

        _controller = new();
        _controller.ControllerContext.HttpContext = httpContext;
    }

    [Fact]
    public async Task DownloadContent_WithValidRequest_ReturnFileStreamResult()
    {
        // Arrange
        var guidId = Guid.Empty;

        var fileContextName = "FooName";
        var fileContext = FileContext.Create(fileContextName, 1);

        var contentStream = new MemoryStream([1], false);
        var fileContent = FileContent.Create(contentStream);
        
        fileContext.SetContent(fileContent);
        var fileContextResult = new OperationResult<FileContext> { Payload = fileContext };

        _mediatorMock.Setup(
            m => m.Send(
                It.Is<GetFileContextWithContentById>(x => x.FileContextId == guidId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileContextResult);

        // Act
        var result = await _controller.DownloadContent(guidId.ToString(), default);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<GetFileContextWithContentById>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.BeOfType<FileStreamResult>()

            .Which.Should().HaveContentType("application/octet-stream")
            .And.HaveFileDownloadName(fileContextName)
            .And.HaveDisabledRangeProcessing()
            .And.HaveNotNullStream()
            
            .Which.Should().BeReadOnly()
            .And.HaveLength(contentStream.Length);
    }

    [Fact]
    public async Task DownloadContent_WithRequestFailure_ReturnBadRequestResponse()
    {
        // Arrange
        var guidId = Guid.Empty;

        var fileContextResult = new OperationResult<FileContext>();

        var errorMessage = "Test unknown error occurred.";
        fileContextResult.AddError((ErrorCode)(-1), errorMessage);

        _mediatorMock.Setup(
            m => m.Send(
                It.Is<GetFileContextWithContentById>(x => x.FileContextId == guidId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileContextResult);

        // Act
        var result = await _controller.DownloadContent(guidId.ToString(), default);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<GetFileContextWithContentById>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.BeOfType<BadRequestObjectResult>()

            .Which.Should().HaveStatusCode(400)
            .And.HaveNotNullValue()
            .And.HaveValueOfType<ErrorResponse>()

            .Which.Should().HaveStatusCode(400)
            .And.HaveStatusPhrase("Bad Request")
            .And.HaveTimeStampCloseTo(DateTime.UtcNow, 1.Seconds())
            .And.HaveSingleError(errorMessage);
    }
}
