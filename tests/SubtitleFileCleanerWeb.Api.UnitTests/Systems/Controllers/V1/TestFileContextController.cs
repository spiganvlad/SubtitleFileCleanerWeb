using AutoMapper;
using FluentAssertions;
using FluentAssertions.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SubtitleFileCleanerWeb.Api.Contracts.Common;
using SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Requests;
using SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Responses;
using SubtitleFileCleanerWeb.Api.Controllers.V1;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Builders;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Extensions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContexts.Commands;
using SubtitleFileCleanerWeb.Application.FileContexts.Queries;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.PostConversion.Commands;
using SubtitleFileCleanerWeb.Application.SubtitleConversion.Commands;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Systems.Controllers.V1;

public class TestFileContextController
{
    private readonly Mock<IMediator> _mediatorMock; 
    private readonly Mock<IMapper> _mapperMock;
    private readonly FileContextController _controller;

    public TestFileContextController()
    {
        _mediatorMock = new Mock<IMediator>();
        _mapperMock = new Mock<IMapper>();

        var httpContext = new HttpContextMockBuilder()
            .SetupIMediator(_mediatorMock)
            .SetupIMapper(_mapperMock)
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

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetFileContextById>(), cancellationToken))
            .ReturnsAsync(mediatorResult);

        _mapperMock.Setup(m => m.Map<FileContext, FileContextResponse>(fileContext))
            .Returns(new FileContextResponse { Name = fileContextName });

        // Act
        var result = await _controller.GetById(Guid.Empty.ToString(), cancellationToken);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetFileContextById>(), It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<FileContext, FileContextResponse>(It.IsAny<FileContext>()), Times.Once);

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

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetFileContextById>(), cancellationToken))
            .ReturnsAsync(mediatorResult);

        // Act
        var result = await _controller.GetById(Guid.Empty.ToString(), cancellationToken);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetFileContextById>(), It.IsAny<CancellationToken>()), Times.Once());
        _mapperMock.Verify(m => m.Map<FileContext, FileContextResponse>(It.IsAny<FileContext>()), Times.Never());

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
    public async Task CreateConvertedContext_WithValidParameters_ReturnOkResponse()
    {
        // Arrange
        var formFileMock = new Mock<IFormFile>();
        var conversionType = ConversionType.Ass;
        var postConversionOptions = new PostConversionOption[2];
        postConversionOptions[0] = PostConversionOption.DeleteTags;
        postConversionOptions[1] = PostConversionOption.ToOneLine;
        var cancellationToken = new CancellationToken();
        
        var formFileStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
        formFileMock.Setup(ff => ff.OpenReadStream())
            .Returns(formFileStream);

        var formFileName = "FooName";
        formFileMock.SetupGet(ff => ff.FileName)
            .Returns(formFileName);

        var convertFile = new ConvertSubtitleFile(formFileStream, conversionType);
        var conversionResultStream = new MemoryStream(new byte[] { 1, 2, 4, 5 });
        var conversionResult = new OperationResult<Stream>() { Payload = conversionResultStream };
        _mediatorMock.Setup(m => m.Send(convertFile, cancellationToken))
            .ReturnsAsync(conversionResult);

        var postConvertFile = new PostConvertFile(conversionResultStream, conversionType, postConversionOptions);
        var postConversionResultStream = new MemoryStream(new byte[] { 1, 4 });
        var postConversionResult = new OperationResult<Stream>() { Payload = postConversionResultStream };
        _mediatorMock.Setup(m => m.Send(postConvertFile, cancellationToken))
            .ReturnsAsync(postConversionResult);

        var createContext = new CreateFileContext(formFileName + ".txt", postConversionResultStream);
        var fileContext = FileContext.Create(formFileName);
        var createContextResult = new OperationResult<FileContext> { Payload = fileContext };
        _mediatorMock.Setup(m => m.Send(createContext, cancellationToken))
            .ReturnsAsync(createContextResult);

        var fileContextResponse = new FileContextResponse
        { FileContextId = createContextResult.Payload.FileContextId, Name = createContextResult.Payload.Name };
        _mapperMock.Setup(m => m.Map<FileContext, FileContextResponse>(fileContext))
            .Returns(fileContextResponse);

        var request = new FileContextCreateConverted(formFileMock.Object, postConversionOptions);

        // Act
        var result = await _controller.CreateConvertedContext(conversionType, request, cancellationToken);

        // Assert
        formFileMock.Verify(ff => ff.OpenReadStream(), Times.Once());
        formFileMock.VerifyGet(ff => ff.FileName, Times.Once());

        _mediatorMock.Verify(m => m.Send(It.IsAny<ConvertSubtitleFile>(), It.IsAny<CancellationToken>()), Times.Once());
        _mediatorMock.Verify(m => m.Send(It.IsAny<PostConvertFile>(), It.IsAny<CancellationToken>()), Times.Once());
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFileContext>(), It.IsAny<CancellationToken>()), Times.Once());

        _mapperMock.Verify(m => m.Map<FileContext, FileContextResponse>(It.IsAny<FileContext>()), Times.Once());

        result.Should().NotBeNull().And.BeOfType<OkObjectResult>()
            
            .Which.Should().HaveStatusCode(200)
            .And.HaveNotNullValue()
            .And.HaveValueOfType<FileContextResponse>()
            
            .Which.Should().Be(fileContextResponse);
    }

    [Fact]
    public async Task CreateConvertedContext_WithSubtitleConversionError_ReturnBadRequestResponse()
    {
        // Arrange
        var formFileMock = new Mock<IFormFile>();
        var conversionType = ConversionType.Ass;
        var cancellationToken = new CancellationToken();

        formFileMock.Setup(ff => ff.OpenReadStream())
            .Returns(Stream.Null);

        var convertFile = new ConvertSubtitleFile(Stream.Null, conversionType);
        var conversionResult = new OperationResult<Stream>();
        var errorMessage = "Test unknown error occurred.";
        conversionResult.AddError(ErrorCode.UnknownError, errorMessage);
        _mediatorMock.Setup(m => m.Send(convertFile, cancellationToken))
            .ReturnsAsync(conversionResult);

        var request = new FileContextCreateConverted(formFileMock.Object);

        // Act
        var result = await _controller.CreateConvertedContext(conversionType, request, cancellationToken);

        // Assert
        formFileMock.Verify(ff => ff.OpenReadStream(), Times.Once());
        formFileMock.VerifyGet(ff => ff.FileName, Times.Never());

        _mediatorMock.Verify(m => m.Send(It.IsAny<ConvertSubtitleFile>(), It.IsAny<CancellationToken>()), Times.Once());
        _mediatorMock.Verify(m => m.Send(It.IsAny<PostConvertFile>(), It.IsAny<CancellationToken>()), Times.Never());
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFileContext>(), It.IsAny<CancellationToken>()), Times.Never());

        _mapperMock.Verify(m => m.Map<FileContext, FileContextResponse>(It.IsAny<FileContext>()), Times.Never());


        result.Should().NotBeNull().And.BeOfType<BadRequestObjectResult>()
            
            .Which.Should().HaveStatusCode(400)
            .And.HaveNotNullValue()
            .And.HaveValueOfType<ErrorResponse>()
            
            .Which.Should().HaveStatusCode(400)
            .And.HaveStatusPhrase("Bad Request")
            .And.HaveTimeStampCloseTo(DateTime.UtcNow, 1.Minutes())
            .And.HaveSingleError(errorMessage);
    }

    [Fact]
    public async Task CreateConvertedContext_WithPostConversionError_ReturnBadRequestResponse()
    {
        // Arrange
        var formFileMock = new Mock<IFormFile>();
        var conversionType = ConversionType.Ass;
        var postConversionOptions = new PostConversionOption[] { PostConversionOption.DeleteTags };
        var cancellationToken = new CancellationToken();

        formFileMock.Setup(ff => ff.OpenReadStream())
            .Returns(Stream.Null);

        var convertFile = new ConvertSubtitleFile(Stream.Null, conversionType);
        _mediatorMock.Setup(m => m.Send(convertFile, cancellationToken))
            .ReturnsAsync(new OperationResult<Stream> { Payload = Stream.Null });

        var postConvertFile = new PostConvertFile(Stream.Null, conversionType, postConversionOptions);
        var postConvertResult = new OperationResult<Stream>();
        var errorMessage = "Test unknown error occurred.";
        postConvertResult.AddError(ErrorCode.UnknownError, errorMessage);
        _mediatorMock.Setup(m => m.Send(postConvertFile, cancellationToken))
            .ReturnsAsync(postConvertResult);

        var request = new FileContextCreateConverted(formFileMock.Object, postConversionOptions);

        // Act
        var result = await _controller.CreateConvertedContext(conversionType, request, cancellationToken);

        // Assert
        formFileMock.Verify(ff => ff.OpenReadStream(), Times.Once());
        formFileMock.VerifyGet(ff => ff.FileName, Times.Never());

        _mediatorMock.Verify(m => m.Send(It.IsAny<ConvertSubtitleFile>(), It.IsAny<CancellationToken>()), Times.Once());
        _mediatorMock.Verify(m => m.Send(It.IsAny<PostConvertFile>(), It.IsAny<CancellationToken>()), Times.Once());
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFileContext>(), It.IsAny<CancellationToken>()), Times.Never());

        _mapperMock.Verify(m => m.Map<FileContext, FileContextResponse>(It.IsAny<FileContext>()), Times.Never());

        result.Should().NotBeNull().And.BeOfType<BadRequestObjectResult>()

            .Which.Should().HaveStatusCode(400)
            .And.HaveNotNullValue()
            .And.HaveValueOfType<ErrorResponse>()

            .Which.Should().HaveStatusCode(400)
            .And.HaveStatusPhrase("Bad Request")
            .And.HaveTimeStampCloseTo(DateTime.UtcNow, 1.Minutes())
            .And.HaveSingleError(errorMessage);
    }

    [Fact]
    public async Task CreateConvertedContext_WithFileContextError_ReturnBadRequestError()
    {
        // Arrange
        var formFileMock = new Mock<IFormFile>();
        var conversionType = ConversionType.Ass;
        var cancellationToken = new CancellationToken();

        formFileMock.Setup(ff => ff.OpenReadStream())
            .Returns(Stream.Null);

        formFileMock.SetupGet(ff => ff.FileName)
            .Returns(string.Empty);

        var convertFile = new ConvertSubtitleFile(Stream.Null, conversionType);
        _mediatorMock.Setup(m => m.Send(convertFile, cancellationToken))
            .ReturnsAsync(new OperationResult<Stream> { Payload = Stream.Null });

        var createContext = new CreateFileContext(string.Empty, Stream.Null);
        var createContextResult = new OperationResult<FileContext>();
        var errorMessage = "Test unknown error occurred";
        createContextResult.AddError(ErrorCode.UnknownError, errorMessage);
        _mediatorMock.Setup(m => m.Send(createContext, cancellationToken))
            .ReturnsAsync(createContextResult);

        var request = new FileContextCreateConverted(formFileMock.Object);

        // Act
        var result = await _controller.CreateConvertedContext(conversionType, request, cancellationToken);

        // Assert
        formFileMock.Verify(ff => ff.OpenReadStream(), Times.Once());
        formFileMock.VerifyGet(ff => ff.FileName, Times.Once());

        _mediatorMock.Verify(m => m.Send(It.IsAny<ConvertSubtitleFile>(), It.IsAny<CancellationToken>()), Times.Once());
        _mediatorMock.Verify(m => m.Send(It.IsAny<PostConvertFile>(), It.IsAny<CancellationToken>()), Times.Never());
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateFileContext>(), It.IsAny<CancellationToken>()), Times.Once());

        _mapperMock.Verify(m => m.Map<FileContext, FileContextResponse>(It.IsAny<FileContext>()), Times.Never());

        result.Should().NotBeNull().And.BeOfType<BadRequestObjectResult>()

            .Which.Should().HaveStatusCode(400)
            .And.HaveNotNullValue()
            .And.HaveValueOfType<ErrorResponse>()

            .Which.Should().HaveStatusCode(400)
            .And.HaveStatusPhrase("Bad Request")
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

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateFileContextName>(), cancellationToken))
            .ReturnsAsync(mediatorResult);

        _mapperMock.Setup(m => m.Map<FileContext, FileContextResponse>(fileContext))
            .Returns(new FileContextResponse { Name = fileContextName });

        // Act
        var result = await _controller.UpdateName(Guid.Empty.ToString(), string.Empty, cancellationToken);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateFileContextName>(), It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<FileContext, FileContextResponse>(It.IsAny<FileContext>()), Times.Once());

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

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateFileContextName>(), cancellationToken))
            .ReturnsAsync(mediatorResult);

        // Act
        var result = await _controller.UpdateName(Guid.Empty.ToString(), string.Empty, cancellationToken);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateFileContextName>(), It.IsAny<CancellationToken>()), Times.Once());
        _mapperMock.Verify(m => m.Map<FileContext, FileContextResponse>(It.IsAny<FileContext>()), Times.Never());

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

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateFileContextName>(), cancellationToken))
            .ReturnsAsync(mediatorResult);

        // Act
        var result = await _controller.UpdateName(Guid.Empty.ToString(), string.Empty, cancellationToken);

        // Assert
        _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateFileContextName>(), It.IsAny<CancellationToken>()), Times.Once());
        _mapperMock.Verify(m => m.Map<FileContext, FileContextResponse>(It.IsAny<FileContext>()), Times.Never());

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
