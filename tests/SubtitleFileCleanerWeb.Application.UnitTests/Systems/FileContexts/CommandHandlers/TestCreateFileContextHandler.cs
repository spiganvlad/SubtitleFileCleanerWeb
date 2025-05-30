﻿using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.Commands;
using SubtitleFileCleanerWeb.Application.FileContexts.CommandHandlers;
using SubtitleFileCleanerWeb.Application.FileContexts.Commands;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.FileContexts.CommandHandlers;

public class TestCreateFileContextHandler
{
    private readonly List<FileContext> _fileContexts;
    private readonly Mock<ApplicationDbContext> _dbContextMock;
    private readonly Mock<IMediator> _mediatorMock;

    public TestCreateFileContextHandler()
    {
        _fileContexts = [];

        _dbContextMock = new();
        _dbContextMock.Setup(db => db.FileContexts.Add(It.IsAny<FileContext>()))
            .Callback<FileContext>(_fileContexts.Add);

        _mediatorMock = new();
    }

    [Fact]
    public async Task Handle_WithValidParameters_ReturnValid()
    {
        // Arrange
        var contextName = "FooName";
        byte[] content = [1];
        var contentStream = new MemoryStream(content);

        var mediatorResult = new OperationResult<FileContent>
        {
            Payload = FileContent.Create(new MemoryStream(content, false))
        };

        _mediatorMock.Setup(
            m => m.Send(
                It.Is<CreateFileContent>(x => x.ContentStream == contentStream),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediatorResult);

        var request = new CreateFileContext(contextName, contentStream);

        var handler = new CreateFileContextHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<CreateFileContent>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        _dbContextMock.VerifyGet(
            db => db.FileContexts,
            Times.Once());

        _dbContextMock.Verify(
            db => db.FileContexts.Add(It.IsAny<FileContext>()),
            Times.Once());

        _dbContextMock.Verify(
            db => db.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once());

        var payload = result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            .Which;

        payload.FileContextId.Should().NotBeEmpty();
        payload.FileContent.Should().NotBeNull().And.Be(mediatorResult.Payload);
        payload.Name.Should().NotBeNull().And.Be(request.FileName + ".txt");
        payload.ContentSize.Should().Be(content.Length);
        payload.DateCreated.Should().BeCloseTo(DateTime.UtcNow, 1.Seconds());
        payload.DateModified.Should().BeCloseTo(DateTime.UtcNow, 1.Seconds());

        _fileContexts.Should().ContainSingle()
            .Which.Should().NotBeNull()
            .And.Be(result.Payload);
    }

    [Fact]
    public async Task Handle_WithInvalidParameters_ReturnValidationError()
    {
        // Arrange
        var contextName = string.Empty;
        var contentStream = new MemoryStream([]);

        var request = new CreateFileContext(contextName, contentStream);

        var handler = new CreateFileContextHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<CreateFileContent>(),
                It.IsAny<CancellationToken>()),
            Times.Never());

        _dbContextMock.VerifyGet(
            db => db.FileContexts,
            Times.Never());

        _dbContextMock.Verify(
            db => db.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveMultipleErrors(
                new Error
                {
                    Code = ErrorCode.ValidationError,
                    Message = "File context name cannot be empty."
                },
                new Error
                {
                    Code = ErrorCode.ValidationError,
                    Message = "File context content size must be greater then 0."
                })
            .And.HaveDefaultPayload();

        _fileContexts.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithCreateContentError_RaiseContentError()
    {
        // Arrange
        var mediatorResult = new OperationResult<FileContent>();

        var errorCode = (ErrorCode)(-1);
        var errorMessage = "Test unexpected error occurred.";
        mediatorResult.AddError(errorCode, errorMessage);

        _mediatorMock.Setup(
            m => m.Send(
                It.IsAny<CreateFileContent>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mediatorResult);

        var request = new CreateFileContext("FooName", new MemoryStream(new byte[] { 1 }));

        var handler = new CreateFileContextHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<CreateFileContent>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        _dbContextMock.VerifyGet(
            db => db.FileContexts,
            Times.Never());

        _dbContextMock.Verify(
            db => db.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(errorCode, errorMessage)
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task Handle_WithUnexpectedError_ReturnUnknownError()
    {
        // Arrange
        var exceptionMessage = "Test unexpected error occurred.";
        _mediatorMock.Setup(
            m => m.Send(
                It.IsAny<CreateFileContent>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        var request = new CreateFileContext("FooName", new MemoryStream([1]));

        var handler = new CreateFileContextHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<CreateFileContent>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        _dbContextMock.VerifyGet(
            db => db.FileContexts,
            Times.Never());

        _dbContextMock.Verify(
            db => db.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, exceptionMessage)
            .And.HaveDefaultPayload();
    }
}
