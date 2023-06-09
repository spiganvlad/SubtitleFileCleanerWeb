using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;
using SubtitleFileCleanerWeb.Application.FileContexts.Queries;
using SubtitleFileCleanerWeb.Application.FileContexts.QueryHandlers;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.UnitTests.Fixtures;
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

        _dbContextMock.Setup(x => x.FileContexts)
            .ReturnsDbSet(fileContexts);

        var request = new GetFileContextById(searchedContext.FileContextId);
        var cancellationToken = new CancellationToken();

        var handler = new GetFileContextByIdHandler(_dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _dbContextMock.Verify(x => x.FileContexts, Times.Once);

        result.Should().NotBeNull();
        result.IsError.Should().BeFalse();
        result.Errors.Should().BeEmpty();
        result.Payload.Should().NotBeNull().And.BeOfType<FileContext>().And.Be(searchedContext);
    }

    [Fact]
    public async Task Handle_WithEmptyFiles_ReturnError()
    {
        // Arrange
        var fileContexts = Enumerable.Empty<FileContext>();

        _dbContextMock.Setup(x => x.FileContexts)
            .ReturnsDbSet(fileContexts);

        var request = new GetFileContextById(Guid.Empty);
        var cancellationToken = new CancellationToken();

        var handler = new GetFileContextByIdHandler(_dbContextMock.Object);

        // Act
        var result = await handler.Handle(request, cancellationToken);

        // Assert
        _dbContextMock.Verify(x => x.FileContexts, Times.Once());

        result.Should().NotBeNull();
        result.Payload.Should().BeNull();
        result.IsError.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Code.Should().Be(ErrorCode.NotFound);
        result.Errors[0].Message.Should().Be($"No file context found with id: {Guid.Empty}");
    }
}
