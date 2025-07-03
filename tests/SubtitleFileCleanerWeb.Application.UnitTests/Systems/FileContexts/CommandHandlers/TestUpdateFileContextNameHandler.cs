using AutoFixture.Xunit3;
using MockQueryable.NSubstitute;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContexts.CommandHandlers;
using SubtitleFileCleanerWeb.Application.FileContexts.Commands;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.FileContexts.CommandHandlers;

public class TestUpdateFileContextNameHandler
{
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;
    private readonly ApplicationDbContext _dbContextMock;
    private readonly UpdateFileContextNameHandler _sut;

    public TestUpdateFileContextNameHandler()
    {
        _dbContextMock = Substitute.For<ApplicationDbContext>();
        _sut = new(_dbContextMock);
    }

    [Theory, AutoData]
    public async Task Handle_WithValidRequest_ReturnFileContextResult
        (List<FileContext> fileContexts, string name)
    {
        // Arrange
        var contextToUpdate = fileContexts.Last();
        var request = new UpdateFileContextName(contextToUpdate.FileContextId, name);

        var fileContextsDbSet = fileContexts.AsQueryable().BuildMockDbSet();
        _dbContextMock.FileContexts.Returns(fileContextsDbSet);

        FileContext updatedContext = null!;
        _dbContextMock.FileContexts.Update(Arg.Do<FileContext>(fileContext =>
            updatedContext = fileContext));

        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            
            .Which.Should().Be(contextToUpdate);

        updatedContext.Should().Be(result.Payload);
        await _dbContextMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Theory, AutoData]
    public async Task Handle_WithInvalidFileName_ReturnValidationError
        (List<FileContext> fileContexts)
    {
        // Arrange
        var request = new UpdateFileContextName(fileContexts.Last().FileContextId, string.Empty);

        var fileContextsDbSet = fileContexts.AsQueryable().BuildMockDbSet();
        _dbContextMock.FileContexts.Returns(fileContextsDbSet);

        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveAllErrorsWithCode(ErrorCode.ValidationError)
            .And.HaveDefaultPayload();
    }

    [Theory, AutoData]
    public async Task Handle_WithNonExistentGuidId_ReturnNotFoundError
        (UpdateFileContextName request)
    {
        // Arrange
        var fileContextsDbSet = Array.Empty<FileContext>().AsQueryable().BuildMockDbSet();
        _dbContextMock.FileContexts.Returns(fileContextsDbSet);

        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(
                ErrorCode.NotFound,
                $"No file context found with id: {request.FileContextId}.")
            .And.HaveDefaultPayload();
    }

    [Theory, AutoData]
    public async Task Handle_WithUnexpectedError_ReturnUnknownError
        (UpdateFileContextName request, string message)
    {
        // Arrange
        _dbContextMock.FileContexts.Throws(new Exception(message));

        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, message)
            .And.HaveDefaultPayload();
    }
}
