using AutoFixture.Xunit3;
using MockQueryable.NSubstitute;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContexts.Queries;
using SubtitleFileCleanerWeb.Application.FileContexts.QueryHandlers;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.FileContexts.QueryHandlers;

public class TestGetFileContextByIdHandler
{
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;
    private readonly ApplicationDbContext _dbContextMock;
    private readonly GetFileContextByIdHandler _sut;

    public TestGetFileContextByIdHandler()
    {
        _dbContextMock = Substitute.For<ApplicationDbContext>();
        _sut = new(_dbContextMock);
    }

    [Theory, AutoData]
    public async Task Handle_WithExistingId_ReturnFileContextResult
        (List<FileContext> fileContexts)
    {
        // Arrange
        var searchedContext = fileContexts.Last();
        var request = new GetFileContextById(searchedContext.FileContextId);

        var fileContextsDbSet = fileContexts.AsQueryable().BuildMockDbSet();
        _dbContextMock.FileContexts.Returns(fileContextsDbSet);

        // Act
        var result = await _sut.Handle(request, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.NotBeInErrorState()
            .And.HaveNoErrors()
            .And.HaveNotDefaultPayload()
            
            .Which.Should().Be(searchedContext);
    }

    [Theory, AutoData]
    public async Task Handle_WithNonExistentId_ReturnNotFoundError
        (GetFileContextById request)
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
    public async Task Handle_WithUnknownError_ReturnUnknownError
        (GetFileContextById request, string message)
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
