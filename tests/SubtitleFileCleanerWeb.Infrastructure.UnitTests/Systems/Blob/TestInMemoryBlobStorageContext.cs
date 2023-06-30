using FluentAssertions;
using SubtitleFileCleanerWeb.Infrastructure.Blob;
using SubtitleFileCleanerWeb.Infrastructure.Exceptions;

namespace SubtitleFileCleanerWeb.Infrastructure.UnitTests.Systems.Blob;

public class TestInMemoryBlobStorageContext
{
    [Fact]
    public async Task GetContentStreamAsync_WithExistedPath_ReturnValidStream()
    {
        // Arrange
        var path = string.Empty;
        var contentStream = new MemoryStream();
        var cancellationToken = new CancellationToken();

        var inMemoryStorage = new InMemoryBlobStorageContext();
        inMemoryStorage.StorageContext.Add(path, contentStream);

        // Act
        var result = await inMemoryStorage.GetContentStreamAsync(path, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Length.Should().Be(contentStream.Length);
    }

    [Fact]
    public async Task GetContentStreamAsync_WithNonExistentPath_ReturnNull()
    {
        // Arrange
        var path = string.Empty;
        var cancellationToken = new CancellationToken();

        var inMemoryStorage = new InMemoryBlobStorageContext();

        // Act
        var result = await inMemoryStorage.GetContentStreamAsync(path, cancellationToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateContentAsync_WithNonExistentPath_AddValid()
    {
        // Arrange
        var path = string.Empty;
        var contentStream = new MemoryStream();
        var cancellationToken = new CancellationToken();

        var inMemoryStorage = new InMemoryBlobStorageContext();

        // Act
        await inMemoryStorage.CreateContentAsync(path, contentStream, cancellationToken);

        // Assert
        inMemoryStorage.StorageContext.Should().NotBeEmpty()
            .And.ContainSingle()
            .And.ContainKey(path)
            .And.ContainValue(contentStream);
    }

    [Fact]
    public async Task CreateContentAsync_WithExistentPath_ThrowException()
    {
        // Arrange
        var path = string.Empty;
        var contentStream = new MemoryStream();
        var cancellationToken = new CancellationToken();

        var inMemoryStorage = new InMemoryBlobStorageContext();
        inMemoryStorage.StorageContext.Add(path, contentStream);

        // Act
        var act = async () =>
        {
            await inMemoryStorage.CreateContentAsync(path, contentStream, cancellationToken);
        };

        // Assert
        await act.Should().ThrowAsync<BlobStorageOperationException>()
            .WithMessage($"Content already exists on path: {path}");
    }

    [Fact]
    public async Task DeleteContentAsync_WithExistentPath_ReturnValid()
    {
        // Arrange
        var path = string.Empty;
        var contentStream = new MemoryStream();
        var cancellationToken = new CancellationToken();

        var inMemoryStorage = new InMemoryBlobStorageContext();
        inMemoryStorage.StorageContext.Add(path, contentStream);

        // Act
        await inMemoryStorage.DeleteContentAsync(path, cancellationToken);

        // Assert
        inMemoryStorage.StorageContext.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteContentAsync_WithNonExistentPath_ThrowException()
    {
        // Arrange
        var path = string.Empty;
        var cancellationToken = new CancellationToken();

        var inMemoryStorage = new InMemoryBlobStorageContext();

        // Act
        var act = async () =>
        {
            await inMemoryStorage.DeleteContentAsync(path, cancellationToken);
        };

        // Assert
        await act.Should().ThrowAsync<BlobStorageOperationException>().WithMessage($"No blob content was found on path: {path}");
    }
}
