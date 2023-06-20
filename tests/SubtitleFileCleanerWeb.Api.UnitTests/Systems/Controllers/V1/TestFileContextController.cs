using AutoMapper;
using FluentAssertions;
using FluentAssertions.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SubtitleFileCleanerWeb.Api.Contracts.Common;
using SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Responses;
using SubtitleFileCleanerWeb.Api.Controllers.V1;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Builders;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Extensions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContexts.Commands;
using SubtitleFileCleanerWeb.Application.FileContexts.Queries;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Systems.Controllers.V1;

public class TestFileContextController
{
    private readonly Mock<IMediator> _mediator;
    private readonly Mock<IMapper> _mapper;
    private readonly FileContextController _controller;

    public TestFileContextController()
    {
        _mediator = new Mock<IMediator>();
        _mapper = new Mock<IMapper>();

        var httpContext = new HttpContextMockBuilder()
            .SetupIMediator(_mediator)
            .SetupIMapper(_mapper)
            .Build();

        _controller = new FileContextController();
        _controller.ControllerContext.HttpContext = httpContext;
    }
    
    [Fact]
    public async Task GetById_WithExistingId_ReturnOkObjectResult()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var fileContextName = "FooName";

        var fileContext = FileContext.Create(fileContextName);
        var mediatorResult = new OperationResult<FileContext> { Payload = fileContext };

        _mediator.Setup(m => m.Send(It.IsAny<GetFileContextById>(), cancellationToken))
            .ReturnsAsync(mediatorResult);

        _mapper.Setup(m => m.Map<FileContext, FileContextResponse>(fileContext))
            .Returns(new FileContextResponse { Name = fileContextName });

        // Act
        var result = await _controller.GetById(Guid.Empty, cancellationToken);

        // Assert
        _mediator.Verify(m => m.Send(It.IsAny<GetFileContextById>(), It.IsAny<CancellationToken>()), Times.Once);
        _mapper.Verify(m => m.Map<FileContext, FileContextResponse>(It.IsAny<FileContext>()), Times.Once);

        result.Should().NotBeNull().And.BeOfType<OkObjectResult>()

            .Which.Should().HaveStatusCode(200)
            .And.HaveNotNullValue()
            .And.HaveValueOfType<FileContextResponse>()

            .Which.Name.Should().Be(fileContextName);
    }

    [Fact]
    public async Task GetById_WithNonExistingId_ReturnNotFoundResponse()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var errorMessage = "Test not found error message";

        var mediatorResult = new OperationResult<FileContext>();
        mediatorResult.AddError(ErrorCode.NotFound, errorMessage);

        _mediator.Setup(m => m.Send(It.IsAny<GetFileContextById>(), cancellationToken))
            .ReturnsAsync(mediatorResult);

        // Act
        var result = await _controller.GetById(Guid.Empty, cancellationToken);

        // Assert
        _mediator.Verify(m => m.Send(It.IsAny<GetFileContextById>(), It.IsAny<CancellationToken>()), Times.Once());
        _mapper.Verify(m => m.Map<FileContext, FileContextResponse>(It.IsAny<FileContext>()), Times.Never());

        result.Should().NotBeNull().And.BeOfType<NotFoundObjectResult>()

            .Which.Should().HaveStatusCode(404)
            .And.HaveNotNullValue()
            .And.HaveValueOfType<ErrorResponse>()

            .Which.Should().HaveStatusCode(404)
            .And.HaveStatusPhrase("Not Found")
            .And.HaveTimeStampCloseTo(DateTime.UtcNow, 1.Minutes())
            .And.HaveSingleError(errorMessage);
    }

    [Fact]
    public async Task UpdateName_WithValidParameters_ReturnOkObjectResponse()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var fileContextName = "FooName";

        var fileContext = FileContext.Create(fileContextName);
        var mediatorResult = new OperationResult<FileContext> { Payload = fileContext };

        _mediator.Setup(m => m.Send(It.IsAny<UpdateFileContextName>(), cancellationToken))
            .ReturnsAsync(mediatorResult);

        _mapper.Setup(m => m.Map<FileContext, FileContextResponse>(fileContext))
            .Returns(new FileContextResponse { Name = fileContextName });

        // Act
        var result = await _controller.UpdateName(Guid.Empty, string.Empty, cancellationToken);

        // Assert
        _mediator.Verify(m => m.Send(It.IsAny<UpdateFileContextName>(), It.IsAny<CancellationToken>()), Times.Once);
        _mapper.Verify(m => m.Map<FileContext, FileContextResponse>(It.IsAny<FileContext>()), Times.Once());

        result.Should().NotBeNull().And.BeOfType<OkObjectResult>()

            .Which.Should().HaveStatusCode(200)
            .And.HaveNotNullValue()
            .And.HaveValueOfType<FileContextResponse>()

            .Which.Name.Should().Be(fileContextName);
    }

    [Fact]
    public async Task UpdateName_WithNonExistingId_ReturnNotFoundResponse()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var errorMessage = "Test not found error message";

        var mediatorResult = new OperationResult<FileContext>();
        mediatorResult.AddError(ErrorCode.NotFound, errorMessage);

        _mediator.Setup(m => m.Send(It.IsAny<UpdateFileContextName>(), cancellationToken))
            .ReturnsAsync(mediatorResult);

        // Act
        var result = await _controller.UpdateName(Guid.Empty, string.Empty, cancellationToken);

        // Assert
        _mediator.Verify(m => m.Send(It.IsAny<UpdateFileContextName>(), It.IsAny<CancellationToken>()), Times.Once());
        _mapper.Verify(m => m.Map<FileContext, FileContextResponse>(It.IsAny<FileContext>()), Times.Never());

        result.Should().NotBeNull().And.BeOfType<NotFoundObjectResult>()
            
            .Which.Should().HaveStatusCode(404)
            .And.HaveNotNullValue()
            .And.HaveValueOfType<ErrorResponse>()
            
            .Which.Should().HaveStatusCode(404)
            .And.HaveStatusPhrase("Not Found")
            .And.HaveTimeStampCloseTo(DateTime.UtcNow, 1.Minutes())
            .And.HaveSingleError(errorMessage);
    }

    [Fact]
    public async Task UpdateName_WithNotValidName_ReturnBadRequestResponse()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var errorMessage = "Test not valid error message";

        var mediatorResult = new OperationResult<FileContext>();
        mediatorResult.AddError(ErrorCode.ValidationError, errorMessage);

        _mediator.Setup(m => m.Send(It.IsAny<UpdateFileContextName>(), cancellationToken))
            .ReturnsAsync(mediatorResult);

        // Act
        var result = await _controller.UpdateName(Guid.Empty, string.Empty, cancellationToken);

        // Assert
        _mediator.Verify(m => m.Send(It.IsAny<UpdateFileContextName>(), It.IsAny<CancellationToken>()), Times.Once());
        _mapper.Verify(m => m.Map<FileContext, FileContextResponse>(It.IsAny<FileContext>()), Times.Never());

        result.Should().NotBeNull().And.BeOfType<BadRequestObjectResult>()

            .Which.Should().HaveStatusCode(400)
            .And.HaveNotNullValue()
            .And.HaveValueOfType<ErrorResponse>()

            .Which.Should().HaveStatusCode(400)
            .And.HaveStatusPhrase("Bad Request")
            .And.HaveTimeStampCloseTo(DateTime.UtcNow, 1.Minutes())
            .And.HaveSingleError(errorMessage);
    }
}
