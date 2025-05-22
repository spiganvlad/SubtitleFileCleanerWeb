using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContexts.Queries;
using SubtitleFileCleanerWeb.Application.FileContexts.QueryHandlers;
using SubtitleFileCleanerWeb.Application.UnitTests.Fixtures;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.FileContexts.QueryHandlers;

public class TestGetFileContextByIdHandler
{
    private readonly Mock<ApplicationDbContext> _dbContextMock;

    public TestGetFileContextByIdHandler()
    {
        _dbContextMock = new();
    }

    [Fact]
    public async Task Handle_WithTestFiles_ReturnValid()
    {
        // Arrange
        var fileContexts = FileContextFixture.GetListOfThree();
        var searchedContext = fileContexts.Last();

        var request = new GetFileContextById(searchedContext.FileContextId);

        _dbContextMock.SetupGet(db => db.FileContexts)
            .ReturnsDbSet(fileContexts);

        var handler = new GetFileContextByIdHandler(_dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _dbContextMock.VerifyGet(
            db => db.FileContexts,
            Times.Once());

        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            
            .Which.Should().Be(searchedContext);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnNotFoundError()
    {
        // Arrange
        var request = new GetFileContextById(Guid.Empty);

        _dbContextMock.SetupGet(db => db.FileContexts)
            .ReturnsDbSet([]);

        var handler = new GetFileContextByIdHandler(_dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _dbContextMock.VerifyGet(
            x => x.FileContexts,
            Times.Once());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.NotFound, $"No file context found with id: {Guid.Empty}.")
            .And.HaveDefaultPayload();
    }

    [Fact]
    public async Task Handle_WithUnknownError_ReturnUnknownError()
    {
        // Arrange
        var request = new GetFileContextById(Guid.Empty);

        var exceptionError = "Test unexcepted error occurred.";
        _dbContextMock.SetupGet(db => db.FileContexts)
            .Throws(new Exception(exceptionError));

        var handler = new GetFileContextByIdHandler(_dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, default);

        // Assert
        _dbContextMock.VerifyGet(
            db => db.FileContexts,
            Times.Once());

        result.Should().NotBeNull()
            .And.BeInErrorState()
            .And.HaveSingleError(ErrorCode.UnknownError, exceptionError)
            .And.HaveDefaultPayload();
    }
}   
