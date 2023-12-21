using FluentAssertions;
using MediatR;
using Moq;
using Moq.EntityFrameworkCore;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.Commands;
using SubtitleFileCleanerWeb.Application.FileContexts.CommandHandlers;
using SubtitleFileCleanerWeb.Application.FileContexts.Commands;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.UnitTests.Fixtures;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.FileContexts.CommandHandlers;

public class TestDeleteFileContextHandler
{
    private readonly List<FileContext> _fileContexts;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ApplicationDbContext> _dbContextMock;

    public TestDeleteFileContextHandler()
    {
        _fileContexts = FileContextFixture.GetListOfThree();

        _mediatorMock = new();

        _dbContextMock = new();
        _dbContextMock.Setup(dbContext => dbContext.FileContexts).
            ReturnsDbSet(_fileContexts);
        _dbContextMock.Setup(db => db.FileContexts.Remove(It.IsAny<FileContext>()))
            .Callback<FileContext>(fc => _fileContexts.Remove(fc));
    }

    [Fact]
    public async Task Handle_WithValidGuidId_ReturnValid()
    {
        // Arrange
        var contextToRemove = _fileContexts.Last();
        var path = "Unauthorized\\" + contextToRemove.FileContextId.ToString();

        _mediatorMock.Setup(
            m => m.Send(
                It.Is<DeleteFileContent>(x => x.Path == path),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OperationResult<bool> { Payload = true });

        var request = new DeleteFileContext(contextToRemove.FileContextId);

        var handler = new DeleteFileContextHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<DeleteFileContent>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        _dbContextMock.VerifyGet(
            db => db.FileContexts,
            Times.Exactly(2));

        _dbContextMock.Verify(
            db => db.FileContexts.Remove(It.IsAny<FileContext>()),
            Times.Once());

        _dbContextMock.Verify(
            db => db.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once());

        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            
            .Which.Should().Be(contextToRemove);

        _fileContexts.Should().HaveCount(2)
            .And.NotContain(contextToRemove);
    }

    [Fact]
    public async Task Handle_WithNonExistentGuidId_ReturnNotFoundError()
    {
        // Arrange
        var request = new DeleteFileContext(Guid.Empty);

        var handler = new DeleteFileContextHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<DeleteFileContent>(),
                It.IsAny<CancellationToken>()),
            Times.Never());

        _dbContextMock.VerifyGet(
            db => db.FileContexts,
            Times.Once());

        _dbContextMock.Verify(
            db => db.FileContexts.Remove(It.IsAny<FileContext>()),
            Times.Never());

        _dbContextMock.Verify(
            db => db.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.NotFound, $"No file context found with id: {request.FileContextId}.")
            .And.HaveDefaultPayload();

        _fileContexts.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithContentDeleteError_RaiseError()
    {
        // Arrange
        var contextToDelete = _fileContexts.Last();
        var path = "Unauthorized\\" + contextToDelete.FileContextId.ToString();

        var fileContentResult = new OperationResult<bool>();

        var errorCode = (ErrorCode)(-1);
        var errorMessage = "Test unexpected error occurred.";
        fileContentResult.AddError(errorCode, errorMessage);
        
        _mediatorMock.Setup(
            m => m.Send(
                It.Is<DeleteFileContent>(x => x.Path == path),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileContentResult);

        var request = new DeleteFileContext(contextToDelete.FileContextId);

        var handler = new DeleteFileContextHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<DeleteFileContent>(),
                It.IsAny<CancellationToken>()),
            Times.Once());

        _dbContextMock.VerifyGet(
            db => db.FileContexts,
            Times.Once());

        _dbContextMock.Verify(
            db => db.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(errorCode, errorMessage)
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task Handle_WithUnknownError_ReturnUnknownError()
    {
        // Arrange
        var exceptionMessage = "Test unexpected error occurred.";
        _dbContextMock.SetupGet(db => db.FileContexts)
            .Throws(new Exception(exceptionMessage));

        var request = new DeleteFileContext(Guid.Empty);

        var handler = new DeleteFileContextHandler(_mediatorMock.Object, _dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _dbContextMock.VerifyGet(
            db => db.FileContexts,
            Times.Once());

        _dbContextMock.Verify(
            db => db.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, exceptionMessage)
            .And.HaveDefaultPayload();
    }
}
