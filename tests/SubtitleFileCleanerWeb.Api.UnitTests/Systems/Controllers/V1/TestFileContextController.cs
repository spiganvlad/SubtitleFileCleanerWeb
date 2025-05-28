using SubtitleFileCleanerWeb.Api.Contracts.Common;
using SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Requests;
using SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Responses;
using SubtitleFileCleanerWeb.Api.Controllers.V1;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Builders;
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
        _mediatorMock = new();
        _mapperMock = new();

        var httpContext = new HttpContextMockObjectBuilder()
            .SetupIMediator(_mediatorMock)
            .SetupIMapper(_mapperMock)
            .Build();

        _controller = new();
        _controller.ControllerContext.HttpContext = httpContext;
    }
    
    [Fact]
    public async Task GetById_WithValidRequest_ReturnOkObjectResult()
    {
        // Arrange
        var guidId = Guid.Empty;
        var name = "FooName";
        var contentSize = 1;

        var fileContext = FileContext.Create(name, contentSize);

        var mediatorResult = new OperationResult<FileContext>
        {
            Payload = fileContext
        };

        _mediatorMock.Setup(
            m => m.Send(
                It.Is<GetFileContextById>(x => x.FileContextId == guidId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediatorResult);

        var mapperResult = new FileContextResponse
        {
            Id = guidId,
            Name = name,
            Size = contentSize
        };

        _mapperMock.Setup(m => m.Map<FileContext, FileContextResponse>(fileContext))
            .Returns(mapperResult);

        // Act
        var result = await _controller.GetById(guidId.ToString(), default);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<GetFileContextById>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        _mapperMock.Verify(
            m => m.Map<FileContext, FileContextResponse>(It.IsAny<FileContext>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.BeOfType<OkObjectResult>()

            .Which.Should().HaveStatusCode(200)
            .And.HaveNotNullValue()
            .And.HaveValueOfType<FileContextResponse>()

            .Which.Should().Be(mapperResult);
    }

    [Fact]
    public async Task GetById_WithRequestFailure_ReturnBadRequestResponse()
    {
        // Arrange
        var guidId = Guid.Empty;

        var mediatorResult = new OperationResult<FileContext>();

        var errorMessage = "Test unknown error occurred.";
        mediatorResult.AddError((ErrorCode)(-1), errorMessage);

        _mediatorMock.Setup(
            m => m.Send(
                It.Is<GetFileContextById>(x => x.FileContextId == guidId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediatorResult);

        // Act
        var result = await _controller.GetById(guidId.ToString(), default);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<GetFileContextById>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        _mapperMock.Verify(
            m => m.Map<FileContext, FileContextResponse>(It.IsAny<FileContext>()),
            Times.Never());

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

    [Fact]
    public async Task CreateConvertedContext_WithValidRequest_ReturnCreatedResponse()
    {
        // Arrange
        var conversionType = ConversionType.Ass;
        PostConversionOption[] postConversionOptions =
        [
            PostConversionOption.DeleteAssTags,
            PostConversionOption.ToOneLine
        ];

        var formFileMock = new Mock<IFormFile>();

        var formFileStream = new MemoryStream([1, 2, 3]);
        formFileMock.Setup(ff => ff.OpenReadStream())
            .Returns(formFileStream);

        var formFileName = "FooName";
        formFileMock.SetupGet(ff => ff.FileName)
            .Returns(formFileName);

        var conversionResultStream = new MemoryStream([1, 2]);
        var conversionResult = new OperationResult<Stream>
        {
            Payload = conversionResultStream
        };

        _mediatorMock.Setup(
            m => m.Send(
                It.Is<ConvertSubtitleFile>(x => 
                    x.ContentStream == formFileStream &&
                    x.ConversionType == conversionType), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversionResult);

        var postConversionResultStream = new MemoryStream([1]);
        var postConversionResult = new OperationResult<Stream>
        {
            Payload = postConversionResultStream
        };

        _mediatorMock.Setup(
            m => m.Send(
                It.Is<PostConvertFile>(x =>
                    x.ContentStream == conversionResultStream &&
                    x.ConversionOptions == postConversionOptions),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(postConversionResult);

        var fileContext = FileContext.Create(formFileName, postConversionResultStream.Length);
        var createContextResult = new OperationResult<FileContext>
        {
            Payload = fileContext
        };

        _mediatorMock.Setup(
            m => m.Send(
                It.Is<CreateFileContext>(x =>
                    x.FileName == formFileName &&
                    x.ContentStream == postConversionResultStream),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createContextResult);

        var fileContextResponse = new FileContextResponse
        {
            Id = fileContext.FileContextId,
            Name = fileContext.Name,
            Size = fileContext.ContentSize
        };

        _mapperMock.Setup(m => m.Map<FileContext, FileContextResponse>(fileContext))
            .Returns(fileContextResponse);

        var request = new CreateFromConversionRequest(formFileMock.Object, postConversionOptions);

        // Act
        var result = await _controller.CreateFromConversion(conversionType, request, default);

        // Assert
        formFileMock.Verify(ff => ff.OpenReadStream(), Times.Once());
        formFileMock.VerifyGet(ff => ff.FileName, Times.Once());

        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<ConvertSubtitleFile>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<PostConvertFile>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<CreateFileContext>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        _mapperMock.Verify(
            m => m.Map<FileContext, FileContextResponse>(It.IsAny<FileContext>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.BeOfType<CreatedAtActionResult>()
            
            .Which.Should().HaveStatusCode(201)
            .And.HaveNotNullValue()
            .And.HaveValueOfType<FileContextResponse>()
            
            .Which.Should().Be(fileContextResponse);
    }

    [Fact]
    public async Task CreateConvertedContext_WithSubtitleConversionRequestFailure_ReturnBadRequestResponse()
    {
        // Arrange
        var conversionType = ConversionType.Ass;

        var formFileMock = new Mock<IFormFile>();

        var nullStream = Stream.Null;
        formFileMock.Setup(ff => ff.OpenReadStream())
            .Returns(nullStream);

        var conversionResult = new OperationResult<Stream>();

        var errorMessage = "Test unknown error occurred.";
        conversionResult.AddError((ErrorCode)(-1), errorMessage);

        _mediatorMock.Setup(
            m => m.Send(
                It.Is<ConvertSubtitleFile>(x => 
                    x.ContentStream == nullStream &&
                    x.ConversionType == conversionType),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversionResult);

        var request = new CreateFromConversionRequest(formFileMock.Object);

        // Act
        var result = await _controller.CreateFromConversion(conversionType, request, default);

        // Assert
        formFileMock.Verify(ff => ff.OpenReadStream(), Times.Once());
        formFileMock.VerifyGet(ff => ff.FileName, Times.Never());

        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<ConvertSubtitleFile>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<PostConvertFile>(),
                It.IsAny<CancellationToken>()),
            Times.Never());

        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<CreateFileContext>(),
                It.IsAny<CancellationToken>()),
            Times.Never());

        _mapperMock.Verify(
            m => m.Map<FileContext, FileContextResponse>(It.IsAny<FileContext>()),
            Times.Never());

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

    [Fact]
    public async Task CreateConvertedContext_WithPostConversionRequestFailure_ReturnBadRequestResponse()
    {
        // Arrange
        var conversionType = ConversionType.Ass;
        PostConversionOption[] postConversionOptions = [PostConversionOption.DeleteAssTags];

        var formFileMock = new Mock<IFormFile>();

        var nullStream = Stream.Null;
        formFileMock.Setup(ff => ff.OpenReadStream())
            .Returns(nullStream);

        _mediatorMock.Setup(
            m => m.Send(
                It.Is<ConvertSubtitleFile>(x =>
                    x.ContentStream == nullStream &&
                    x.ConversionType == conversionType),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OperationResult<Stream> { Payload = nullStream });

        var postConvertResult = new OperationResult<Stream>();

        var errorMessage = "Test unknown error occurred.";
        postConvertResult.AddError((ErrorCode)(-1), errorMessage);

        _mediatorMock.Setup(
            m => m.Send(
                It.Is<PostConvertFile>(x => 
                    x.ContentStream == nullStream &&
                    x.ConversionOptions == postConversionOptions),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(postConvertResult);

        var request = new CreateFromConversionRequest(formFileMock.Object, postConversionOptions);

        // Act
        var result = await _controller.CreateFromConversion(conversionType, request, default);

        // Assert
        formFileMock.Verify(ff => ff.OpenReadStream(), Times.Once());
        formFileMock.VerifyGet(ff => ff.FileName, Times.Never());

        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<ConvertSubtitleFile>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<PostConvertFile>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<CreateFileContext>(),
                It.IsAny<CancellationToken>()),
            Times.Never());

        _mapperMock.Verify(
            m => m.Map<FileContext, FileContextResponse>(It.IsAny<FileContext>()),
            Times.Never());

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

    [Fact]
    public async Task CreateConvertedContext_WithFileContextRequestFailure_ReturnBadRequestError()
    {
        // Arrange
        var conversionType = ConversionType.Ass;

        var formFileMock = new Mock<IFormFile>();

        var nullStream = Stream.Null;
        formFileMock.Setup(ff => ff.OpenReadStream())
            .Returns(nullStream);

        formFileMock.SetupGet(ff => ff.FileName)
            .Returns(string.Empty);

        _mediatorMock.Setup(
            m => m.Send(
                It.Is<ConvertSubtitleFile>(x => 
                    x.ContentStream == nullStream &&
                    x.ConversionType == conversionType),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OperationResult<Stream> { Payload = Stream.Null });

        var createContextResult = new OperationResult<FileContext>();

        var errorMessage = "Test unknown error occurred.";
        createContextResult.AddError((ErrorCode)(-1), errorMessage);

        _mediatorMock.Setup(
            m => m.Send(
                It.Is<CreateFileContext>(x => 
                    x.FileName == string.Empty &&
                    x.ContentStream == nullStream),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createContextResult);

        var request = new CreateFromConversionRequest(formFileMock.Object);

        // Act
        var result = await _controller.CreateFromConversion(conversionType, request, default);

        // Assert
        formFileMock.Verify(ff => ff.OpenReadStream(), Times.Once());
        formFileMock.VerifyGet(ff => ff.FileName, Times.Once());

        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<ConvertSubtitleFile>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<PostConvertFile>(),
                It.IsAny<CancellationToken>()),
            Times.Never());

        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<CreateFileContext>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        _mapperMock.Verify(
            m => m.Map<FileContext, FileContextResponse>(It.IsAny<FileContext>()),
            Times.Never());

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

    [Fact]
    public async Task UpdateName_WithValidRequest_ReturnOkObjectResponse()
    {
        // Arrange
        var guidId = Guid.Empty;
        var name = "FooName";
        var contentSize = 1;

        var fileContext = FileContext.Create(name, contentSize);
        var mediatorResult = new OperationResult<FileContext>
        {
            Payload = fileContext
        };

        _mediatorMock.Setup(
            m => m.Send(
                It.Is<UpdateFileContextName>(x => 
                    x.FileContextId == guidId &&
                    x.FileName == name), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediatorResult);

        var mapperResult = new FileContextResponse 
        { 
            Id = guidId,
            Name = name,
            Size = contentSize
        };

        _mapperMock.Setup(m => m.Map<FileContext, FileContextResponse>(fileContext))
            .Returns(mapperResult);

        var request = new UpdateNameRequest(name);

        // Act
        var result = await _controller.UpdateName(guidId.ToString(), request, default);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<UpdateFileContextName>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        _mapperMock.Verify(
            m => m.Map<FileContext, FileContextResponse>(It.IsAny<FileContext>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.BeOfType<OkObjectResult>()

            .Which.Should().HaveStatusCode(200)
            .And.HaveNotNullValue()
            .And.HaveValueOfType<FileContextResponse>()

            .Which.Should().Be(mapperResult);
    }

    [Fact]
    public async Task UpdateName_WithRequestFailure_ReturnBadRequestResponse()
    {
        // Arrange
        var guidId = Guid.Empty;
        var name = string.Empty;

        var mediatorResult = new OperationResult<FileContext>();
        
        var errorMessage = "Test unknown error occurred.";
        mediatorResult.AddError((ErrorCode)(-1), errorMessage);

        _mediatorMock.Setup(
            m => m.Send(
                It.Is<UpdateFileContextName>(x =>
                    x.FileContextId == guidId &&
                    x.FileName == name),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediatorResult);

        var request = new UpdateNameRequest(name);

        // Act
        var result = await _controller.UpdateName(guidId.ToString(), request, default);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<UpdateFileContextName>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        _mapperMock.Verify(
            m => m.Map<FileContext,FileContextResponse>(It.IsAny<FileContext>()),
            Times.Never());

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

    [Fact]
    public async Task Delete_WithValidRequest_ReturnOkObjectResult()
    {
        // Arrange
        var guidId = Guid.Empty;
        var name = "FooName";
        var contentSize = 1;

        var fileContext = FileContext.Create(name, contentSize);
        var mediatorResult = new OperationResult<FileContext>
        { 
            Payload = fileContext
        };

        _mediatorMock.Setup(
            m => m.Send(
                It.Is<DeleteFileContext>(x => x.FileContextId == guidId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediatorResult);

        var mapperResult = new FileContextResponse
        {
            Id = guidId,
            Name = name,
            Size = contentSize
        };

        _mapperMock.Setup(m => m.Map<FileContext, FileContextResponse>(fileContext))
            .Returns(mapperResult);

        // Act
        var result = await _controller.Delete(guidId.ToString(), default);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<DeleteFileContext>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        _mapperMock.Verify(
            m => m.Map<FileContext, FileContextResponse>(It.IsAny<FileContext>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.BeOfType<OkObjectResult>()
            
            .Which.Should().HaveStatusCode(200)
            .And.HaveNotNullValue()
            .And.HaveValueOfType<FileContextResponse>()
            
            .Which.Should().Be(mapperResult);
    }

    [Fact]
    public async Task Delete_WithRequestFailure_ReturnBadRequestResponse()
    {
        // Arrange
        var guidId = Guid.Empty;

        var mediatorResult = new OperationResult<FileContext>();

        var errorMessage = "Test unknown error occurred.";
        mediatorResult.AddError((ErrorCode)(-1), errorMessage);

        _mediatorMock.Setup(
            m => m.Send(
                It.Is<DeleteFileContext>(x => x.FileContextId == guidId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediatorResult);

        // Act
        var result = await _controller.Delete(guidId.ToString(), default);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<DeleteFileContext>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        _mapperMock.Verify(
            m => m.Map<FileContext, FileContextResponse>(It.IsAny<FileContext>()),
            Times.Never());

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
