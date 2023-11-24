using FluentAssertions;
using FluentAssertions.Extensions;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Domain.Exceptions.FileContextAggregateExceptions;

namespace SubtitleFileCleanerWeb.Domain.UnitTests.Systems.Aggregates.FileContextAggregate;

public class TestFileContext
{
    [Fact]
    public void Create_WithValidParameters_ReturnValid()
    {
        // Arrange
        var name = "FooName";
        var contentSize = 1;

        // Act
        var fileContext = FileContext.Create(name, contentSize);

        // Assert
        fileContext.Should().NotBeNull();
        fileContext.FileContextId.Should().NotBeEmpty();
        fileContext.Name.Should().Be(name);
        fileContext.ContentSize.Should().Be(contentSize);
        fileContext.DateCreated.Should().BeCloseTo(DateTime.UtcNow, 1.Minutes());
        fileContext.DateModified.Should().BeCloseTo(DateTime.UtcNow, 1.Minutes());
    }

    [Fact]
    public void Create_WithNullName_ThrowException()
    {
        // Arrange
        string name = null!;
        var contentSize = 1;

        // Act
        var act = () => FileContext.Create(name, contentSize);

        // Assert
        var exception = act.Should().ThrowExactly<FileContextNotValidException>()
            .WithMessage("File context is not valid.")

            .And.ValidationErrors.Should().ContainSingle()
            .Which.Should().Be("File context name cannot be null.");
    }

    [Fact]
    public void Create_WithEmptyName_ThrowException()
    {
        // Arrange
        var name = string.Empty;
        var contentSize = 1;

        // Act
        var act = () => FileContext.Create(name, contentSize);

        // Assert
        act.Should().ThrowExactly<FileContextNotValidException>()
            .WithMessage("File context is not valid.")

            .And.ValidationErrors.Should().ContainSingle()
            .Which.Should().Be("File context name cannot be empty.");
    }

    [Fact]
    public void Create_WithZeroContentLength_ThrowException()
    {
        // Arrange
        var name = "FooName";
        var contentSize = 0;

        // Act
        var act = () => FileContext.Create(name, contentSize);

        // Assert
        act.Should().ThrowExactly<FileContextNotValidException>()
            .WithMessage("File context is not valid.")

            .And.ValidationErrors.Should().ContainSingle()
            .Which.Should().Be("File context content size must be greater then 0.");  
    }

    [Fact]
    public void UpdateName_WithFooName_UpdatedValidly()
    {
        // Arrange
        var name = "FooName";
        var contentSize = 1;

        var fileContext = FileContext.Create("NameToUpdate", contentSize);

        var idBeforeUpdate = fileContext.FileContextId;
        var dateCreatedBeforeUpdate = fileContext.DateCreated;
        var dateModifiedBeforeUpdate = fileContext.DateModified;

        // Act
        fileContext.UpdateName(name);

        // Assert
        fileContext.FileContextId.Should().Be(idBeforeUpdate);
        fileContext.Name.Should().Be(name);
        fileContext.ContentSize.Should().Be(contentSize);
        fileContext.DateCreated.Should().Be(dateCreatedBeforeUpdate);
        fileContext.DateModified.Should().NotBe(dateModifiedBeforeUpdate)
            .And.BeCloseTo(DateTime.UtcNow, 1.Minutes());
    }

    [Fact]
    public void UpdateName_WithNullName_ThrowException()
    {
        // Arrange
        string name = null!;
        var contentSize = 1;
        var fileContext = FileContext.Create("NameToUpdate", contentSize);

        // Act
        var act = () => fileContext.UpdateName(name);

        // Assert
        act.Should().ThrowExactly<FileContextNotValidException>()
            .WithMessage("Cannot update file context. File context name is not valid.")

            .And.ValidationErrors.Should().ContainSingle()
            .Which.Should().Be("The provided name is either null or white space.");
    }

    [Fact]
    public void UpdateName_WithEmptyName_ThrowException()
    {
        // Arrange
        var name = string.Empty;
        var contentSize = 1;
        var fileContext = FileContext.Create("NameToUpdate", contentSize);

        // Act
        var act = () => fileContext.UpdateName(name);

        // Assert
        act.Should().ThrowExactly<FileContextNotValidException>()
            .WithMessage("Cannot update file context. File context name is not valid.")

            .And.ValidationErrors.Should().ContainSingle()
            .Which.Should().Be("The provided name is either null or white space.");
    }

    [Fact]
    public void SetContent_WithNotSetContent_SetValid()
    {
        // Arrange
        var content = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }, false);
        var fileContent = FileContent.Create(content);

        var fileContext = FileContext.Create("FooName", content.Length);

        // Act
        fileContext.SetContent(fileContent);

        // Assert
        fileContent.Content.Should().NotBeNull()
            .And.HaveLength(content.Length)
            .And.BeReadOnly();
    }

    [Fact]
    public void SetContent_WithAlreadySetContent_ThrowException()
    {
        // Arrange
        var content = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }, false);
        var fileContent = FileContent.Create(content);

        var fileContext = FileContext.Create("FooName", content.Length);
        fileContext.SetContent(fileContent);

        // Act
        var act = () => fileContext.SetContent(fileContent);

        // Assert
        act.Should().ThrowExactly<FileContentAlreadySetException>()
            .WithMessage("Cannot set file content. It is already set.");
    }
}
