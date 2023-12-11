using FluentAssertions;
using SubtitleFileCleanerWeb.Infrastructure.Blob.InMemory;
using SubtitleFileCleanerWeb.Infrastructure.Exceptions;

namespace SubtitleFileCleanerWeb.Infrastructure.UnitTests.Systems.Blob;

public class TestInMemoryBlobStorageContext
{
    [Fact]
    public async Task GetContentStreamAsync_WithExistedPath_ReturnValidStream()
    {
        // Arrange
        var path = string.Empty;
        var content = new byte[3];

        var inMemoryStorage = new InMemoryBlobStorageContext();
        inMemoryStorage.StorageContext.Add(path, content);

        // Act
        var result = await inMemoryStorage.GetContentStreamAsync(path, default);

        // Assert
        result.Should().NotBeNull()
            .And.HaveLength(content.Length);
    }

    [Fact]
    public async Task GetContentStreamAsync_WithNonExistentPath_ReturnNull()
    {
        // Arrange
        var path = string.Empty;

        var inMemoryStorage = new InMemoryBlobStorageContext();

        // Act
        var result = await inMemoryStorage.GetContentStreamAsync(path, default);

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

        var inMemoryStorage = new InMemoryBlobStorageContext();

        // Act
        await inMemoryStorage.CreateContentAsync(path, contentStream, default);

        // Assert
        inMemoryStorage.StorageContext.Should().NotBeEmpty()
            .And.ContainSingle()
            .And.ContainKey(path);

        inMemoryStorage.StorageContext[path].Should().NotBeNull()
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

        var inMemoryStorage = new InMemoryBlobStorageContext();
        inMemoryStorage.StorageContext.Add(path, content);

        // Act
        var act = async () =>
            await inMemoryStorage.CreateContentAsync(path, contentStream, default);

        // Assert
        await act.Should().ThrowAsync<BlobStorageOperationException>()
            .WithMessage($"Content already exists on path: {path}");
    }

    [Fact]
    public async Task DeleteContentAsync_WithExistentPath_ReturnValid()
    {
        // Arrange
        var path = string.Empty;
        var content = new byte[3];

        var inMemoryStorage = new InMemoryBlobStorageContext();
        inMemoryStorage.StorageContext.Add(path, content);

        // Act
        await inMemoryStorage.DeleteContentAsync(path, default);

        // Assert
        inMemoryStorage.StorageContext.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteContentAsync_WithNonExistentPath_ThrowException()
    {
        // Arrange
        var path = string.Empty;

        var inMemoryStorage = new InMemoryBlobStorageContext();

        // Act
        var act = async () => await inMemoryStorage.DeleteContentAsync(path, default);


        // Assert
        await act.Should().ThrowAsync<BlobStorageOperationException>()
            .WithMessage($"No blob content was found on path: {path}");
    }
}
