using FluentAssertions;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Domain.Exceptions;

namespace SubtitleFileCleanerWeb.Domain.UnitTests.Systems.Aggregates.FileContextAggregate;

public class TestFileContent
{
    [Fact]
    public void Create_WithValidStream_ReturnValid()
    {
        // Arrange
        var contentStream = new MemoryStream(new byte[] { 1 }, false);

        // Act
        var fileContent = FileContent.Create(contentStream);

        // Assert
        fileContent.Should().NotBeNull();

        fileContent.Content.Should().NotBeNull()
            .And.BeReadOnly()
            .And.HaveLength(contentStream.Length);
    }

    [Fact]
    public void Create_WithNullBytes_ThrowException()
    {
        // Arrange
        MemoryStream contentStream = null!;

        // Act
        var act = () => FileContent.Create(contentStream);

        // Assert
        act.Should().ThrowExactly<FileContentNotValidException>()
            .WithMessage("File content not valid.")

            .And.ValidationErrors.Should().ContainSingle()
            .Which.Should().Be("File content stream cannot be null.");
    }

    [Fact]
    public void Create_WithEmptyBytes_ThrowException()
    {
        // Arrange
        var contentStream = new MemoryStream(Array.Empty<byte>());

        // Act
        var act = () => FileContent.Create(contentStream);

        // Assert
        act.Should().ThrowExactly<FileContentNotValidException>()
            .WithMessage("File content not valid.")

            .And.ValidationErrors.Should().ContainSingle()
            .Which.Should().Be("File content stream cannot be empty.");
    }

    [Fact]
    public void Create_WithWritableStream_ThrowException()
    {
        // Arrange
        var contentStream = new MemoryStream(new byte[] { 1 }, true);

        // Act
        var act = () => FileContent.Create(contentStream);

        // Assert
        act.Should().ThrowExactly<FileContentNotValidException>()
            .WithMessage("File content not valid.")
            
            .And.ValidationErrors.Should().ContainSingle()
            .Which.Should().Be("File content stream must be readonly.");
    }
}
