using SubtitleFileCleanerWeb.Infrastructure.Blob.InMemory;
using SubtitleFileCleanerWeb.Infrastructure.Exceptions;

namespace SubtitleFileCleanerWeb.Infrastructure.UnitTests.Systems.Blob;

public class TestInMemoryBlobStorageContext
{
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;
    private readonly InMemoryBlobStorageContext _sut = new();

    [Fact]
    public async Task GetContentStreamAsync_WithExistedPath_ReturnValidStream()
    {
        // Arrange
        var path = string.Empty;
        var content = new byte[3];

        _sut.StorageContext.Add(path, content);

        // Act
        var result = await _sut.GetContentStreamAsync(path, _cancellationToken);

        // Assert
        result.Should().NotBeNull()
            .And.HaveLength(content.Length);
    }

    [Fact]
    public async Task GetContentStreamAsync_WithNonExistentPath_ReturnNull()
    {
        // Arrange
        var path = string.Empty;

        // Act
        var result = await _sut.GetContentStreamAsync(path, _cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateContentAsync_WithNonExistentPath_AddValid()
    {
        // Arrange
        var path = string.Empty;
        var content = new byte[3];
        var contentStream = new MemoryStream(content);

        // Act
        await _sut.CreateContentAsync(path, contentStream, _cancellationToken);

        // Assert
        _sut.StorageContext.Should().NotBeEmpty()
            .And.ContainSingle()
            .And.ContainKey(path);

        _sut.StorageContext[path].Should().NotBeNull()
            .And.HaveCount(content.Length)
            .And.ContainInOrder(content);
    }

    [Fact]
    public async Task CreateContentAsync_WithExistentPath_ThrowException()
    {
        // Arrange
        var path = string.Empty;
        var content = new byte[3];
        var contentStream = new MemoryStream(content);

        _sut.StorageContext.Add(path, content);

        // Act
        var act = async () =>
            await _sut.CreateContentAsync(path, contentStream, _cancellationToken);

        // Assert
        await act.Should().ThrowAsync<BlobStorageOperationException>()
            .WithMessage($"Content already exists on path: {path}.");
    }

    [Fact]
    public async Task DeleteContentAsync_WithExistentPath_ReturnValid()
    {
        // Arrange
        var path = string.Empty;
        var content = new byte[3];

        _sut.StorageContext.Add(path, content);

        // Act
        await _sut.DeleteContentAsync(path, _cancellationToken);

        // Assert
        _sut.StorageContext.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteContentAsync_WithNonExistentPath_ThrowException()
    {
        // Arrange
        var path = string.Empty;

        // Act
        var act = async () => await _sut.DeleteContentAsync(path, _cancellationToken);


        // Assert
        await act.Should().ThrowAsync<BlobStorageOperationException>()
            .WithMessage($"No blob content was found on path: {path}.");
    }
}
