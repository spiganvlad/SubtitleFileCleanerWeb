using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContexts.Queries;
using SubtitleFileCleanerWeb.Application.FileContexts.QueryHandlers;
using SubtitleFileCleanerWeb.Application.UnitTests.Fixtures;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.FluentOperationResult;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Systems.FileContexts.QueryHandlers;

public class TestGetFileContextByIdHandler
{
    private readonly Mock<ApplicationDbContext> _dbContextMock;

    public TestGetFileContextByIdHandler()
    {
        _dbContextMock = new Mock<ApplicationDbContext>();
    }

    [Fact]
    public async Task Handle_WithTestFiles_ReturnValid()
    {
        // Arrange
        var fileContexts = FileContextFixture.GetListOfThree();
        var searchedContext = fileContexts.Last();

        _dbContextMock.Setup(x => x.FileContexts).ReturnsDbSet(fileContexts);
        var handler = new GetFileContextByIdHandler(_dbContextMock.Object);

        var request = new GetFileContextById(searchedContext.FileContextId);
        var cancellationToken = new CancellationToken();

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _dbContextMock.Verify(x => x.FileContexts, Times.Once);

        result.Should().NotBeNull().And.ContainsNoErrors();
        result.Payload.Should().NotBeNull().And.Be(searchedContext);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnNotFoundError()
    {
        // Arrange
        _dbContextMock.Setup(x => x.FileContexts)
            .ReturnsDbSet(Enumerable.Empty<FileContext>());
        var handler = new GetFileContextByIdHandler(_dbContextMock.Object);

        var request = new GetFileContextById(Guid.Empty);
        var cancellationToken = new CancellationToken();

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _dbContextMock.Verify(x => x.FileContexts, Times.Once());

        result.Should().ContainSingleError(ErrorCode.NotFound, $"No file context found with id: {Guid.Empty}");
    }
}   
