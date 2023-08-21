using FluentAssertions;
using SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Requests;
using SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Requests.Validators;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Extensions;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Systems.Contracts.FileContexts.Requests.Validators;

public class TestUpdateNameValidator
{
    [Fact]
    public void Validate_WithValidName_ReturnValid()
    {
        // Arrange
        var name = "FooName";

        var request = new UpdateNameRequest(name);

        var validator = new UpdateNameValidator();

        // Act
        var result = validator.Validate(request);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNullName_ReturnInvalid()
    {
        // Arrange
        string name = null!;

        var request = new UpdateNameRequest(name);

        var validator = new UpdateNameValidator();

        // Act
        var result = validator.Validate(request);

        // Assert
        result.Should().NotBeNull()
            .And.NotBeValid()
            .And.HaveSingleError("The update name must be provided for the request.");
    }

    [Fact]
    public void Validate_WithEmptyName_ReturnInvalid()
    {
        // Arrange
        var name = string.Empty;

        var request = new UpdateNameRequest(name);

        var validator = new UpdateNameValidator();

        // Act
        var result = validator.Validate(request);

        // Assert
        result.Should().NotBeNull()
            .And.NotBeValid()
            .And.HaveSingleError("Update name must not be empty.");
    }
}
