using FluentAssertions;
using FluentAssertions.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SubtitleFileCleanerWeb.Api.Contracts.Common;
using SubtitleFileCleanerWeb.Api.Controllers.V1;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Builders;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Extensions;
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
        _mediatorMock = new Mock<IMediator>();

        var httpContext = new HttpContextMockBuilder()
            .SetupIMediator(_mediatorMock)
            .Build();

        _controller = new FileContentController();
        _controller.ControllerContext.HttpContext = httpContext;
    }

    [Fact]
    public async Task DownloadContent_WithExistedId_ReturnFileStreamResult()
    {
        // Arrange
        var id = Guid.Empty.ToString();
        var cancellationToken = new CancellationToken();

        var fileContextName = "FooName";
        var fileContext = FileContext.Create(fileContextName);

        var contentStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }, false);
        var fileContent = FileContent.Create(contentStream);

        var getContextWithContent = new GetFileContextWithContentById(Guid.Parse(id));
        var fileContextResult = new OperationResult<FileContext> { Payload = fileContext };
        fileContextResult.Payload.SetContent(fileContent);
        _mediatorMock.Setup(m => m.Send(getContextWithContent, cancellationToken))
            .ReturnsAsync(fileContextResult);

        // Act
        var result = await _controller.DownloadContent(id, cancellationToken);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetFileContextWithContentById>(), It.IsAny<CancellationToken>()), Times.Once());

        var x = result.Should().NotBeNull().And.BeOfType<FileStreamResult>()

            .Which.Should().HaveContentType("application/octet-stream")
            .And.HaveFileDownloadName(fileContextName)
            .And.HaveDisabledRangeProcessing()
            .And.HaveNotNullStream()
            
            .Which.Should().BeReadOnly()
            .And.HaveLength(contentStream.Length);
    }

    [Fact]
    public async Task DownloadContent_WithNonExistentId_ReturnNotFoundResponse()
    {
        // Arrange
        var id = Guid.Empty.ToString();
        var cancellationToken = new CancellationToken();

        var getContextWithContent = new GetFileContextWithContentById(Guid.Parse(id));
        var errorMessage = "Test content not found error.";
        var fileContextResult = new OperationResult<FileContext>();
        fileContextResult.AddError(ErrorCode.NotFound, errorMessage);
        _mediatorMock.Setup(m => m.Send(getContextWithContent, cancellationToken))
            .ReturnsAsync(fileContextResult);

        // Act
        var result = await _controller.DownloadContent(id, cancellationToken);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetFileContextWithContentById>(), It.IsAny<CancellationToken>()), Times.Once());

        result.Should().NotBeNull().And.BeOfType<NotFoundObjectResult>()

            .Which.Should().HaveStatusCode(404)
            .And.HaveNotNullValue()
            .And.HaveValueOfType<ErrorResponse>()

            .Which.Should().HaveStatusCode(404)
            .And.HaveStatusPhrase("Not Found")
            .And.HaveTimeStampCloseTo(DateTime.UtcNow, 1.Minutes())
            .And.HaveSingleError(errorMessage);
    }
}
