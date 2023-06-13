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
        var contentStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }, false);

        // Act
        var fileContent = FileContent.Create(contentStream);

        // Assert
        fileContent.Should().NotBeNull();
        fileContent.Content.Should().NotBeNull().And.BeReadOnly();
        fileContent.Content.Length.Should().Be(contentStream.Length);
    }

    [Fact]
    public void Create_WithNullBytes_ThrowException()
    {
        // Arrange
        MemoryStream contentStream = null!;

        // Act
        var act = () =>
        {
            var fileContent = FileContent.Create(contentStream);
        };

        // Assert
        var exception = act.Should().ThrowExactly<FileContentNotValidException>()
            .WithMessage("File content not valid").And;

        exception.ValidationErrors.Should().ContainSingle();
        exception.ValidationErrors[0].Should().Be("File content stream cannot be null");
    }

    [Fact]
    public void Create_WithEmptyBytes_ThrowException()
    {
        // Arrange
        var contentStream = new MemoryStream(Array.Empty<byte>());

        // Act
        var act = () =>
        {
            var fileContent = FileContent.Create(contentStream);
        };

        // Assert
        var exception = act.Should().ThrowExactly<FileContentNotValidException>()
            .WithMessage("File content not valid").And;

        exception.ValidationErrors.Should().ContainSingle();
        exception.ValidationErrors[0].Should().Be("File content stream cannot be empty");
    }

    [Fact]
    public void Create_WithWritableStream_ThrowException()
    {
        // Arrange
        var contentStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }, true);

        // Act
        var act = () =>
        {
            var fileContent = FileContent.Create(contentStream);
        };

        // Assert
        var exception = act.Should().ThrowExactly<FileContentNotValidException>()
            .WithMessage("File content not valid").And;

        exception.ValidationErrors.Should().ContainSingle();
        exception.ValidationErrors[0].Should().Be("File content stream must be readonly");
    }
}
