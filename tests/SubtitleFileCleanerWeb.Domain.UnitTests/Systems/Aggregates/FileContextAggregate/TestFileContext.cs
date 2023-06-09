using FluentAssertions;
using FluentAssertions.Extensions;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Domain.Exceptions;

namespace SubtitleFileCleanerWeb.Domain.UnitTests.Systems.Aggregates.FileContextAggregate;

public class TestFileContext
{
    [Fact]
    public void Create_WithFooName_ReturnValid()
    {
        // Arrange
        var name = "FooName";

        // Act
        var fileContext = FileContext.Create(name);

        // Assert
        fileContext.Should().NotBeNull();
        fileContext.FileContextId.Should().NotBeEmpty();
        fileContext.Name.Should().Be(name);
        fileContext.DateCreated.Should().BeCloseTo(DateTime.UtcNow, 1.Minutes());
        fileContext.DateModified.Should().BeCloseTo(DateTime.UtcNow, 1.Minutes());
    }

    [Fact]
    public void Create_WithNullName_ThrowException()
    {
        // Arrange
        string name = null!;

        // Act
        var act = () =>
        {
            var fileContext = FileContext.Create(name);
        };

        // Assert
        var exception = act.Should().ThrowExactly<FileContextNotValidException>()
            .WithMessage("File context is not valid").And;

        exception.ValidationErrors.Should().ContainSingle();
        exception.ValidationErrors[0].Should().Be("File context name cannot be null");
    }

    [Fact]
    public void Create_WithEmptyName_ThrowException()
    {
        // Arrange
        var name = string.Empty;

        // Act
        var act = () =>
        {
            var fileContext = FileContext.Create(name);
        };

        // Assert
        var exception = act.Should().ThrowExactly<FileContextNotValidException>()
            .WithMessage("File context is not valid").And;

        exception.ValidationErrors.Should().ContainSingle();
        exception.ValidationErrors[0].Should().Be("File context name cannot be empty");
    }

    [Fact]
    public void UpdateName_WithFooName_UpdatedValidly()
    {
        // Arrange
        var name = "FooName";

        var fileContext = FileContext.Create("NameToUpdate");

        var idBeforeUpdate = fileContext.FileContextId;
        var dateCreatedBeforeUpdate = fileContext.DateCreated;
        var dateModifiedBeforeUpdate = fileContext.DateModified;

        // Act
        fileContext.UpdateName(name);

        // Assert
        fileContext.FileContextId.Should().Be(idBeforeUpdate);
        fileContext.Name.Should().Be(name);
        fileContext.DateCreated.Should().Be(dateCreatedBeforeUpdate);
        fileContext.DateModified.Should().NotBe(dateModifiedBeforeUpdate)
            .And.BeCloseTo(DateTime.UtcNow, 1.Minutes());
    }

    [Fact]
    public void UpdateName_WithNullName_ThrowException()
    {
        // Arrange
        string name = null!;
        var fileContext = FileContext.Create("NameToUpdate");

        // Act
        var act = () =>
        {
            fileContext.UpdateName(name);
        };

        // Assert
        var exception = act.Should().ThrowExactly<FileContextNotValidException>()
            .WithMessage("Cannot update file context. File context name is not valid").And;

        exception.ValidationErrors.Should().ContainSingle();
        exception.ValidationErrors[0].Should().Be("The provided name is either null or white space");
    }

    [Fact]
    public void UpdateName_WithEmptyName_ThrowException()
    {
        // Arrange
        var name = string.Empty;
        var fileContext = FileContext.Create("NameToUpdate");

        // Act
        var act = () =>
        {
            fileContext.UpdateName(name);
        };

        // Assert
        var exception = act.Should().ThrowExactly<FileContextNotValidException>()
            .WithMessage("Cannot update file context. File context name is not valid").And;

        exception.ValidationErrors.Should().ContainSingle();
        exception.ValidationErrors[0].Should().Be("The provided name is either null or white space");
    }
}
