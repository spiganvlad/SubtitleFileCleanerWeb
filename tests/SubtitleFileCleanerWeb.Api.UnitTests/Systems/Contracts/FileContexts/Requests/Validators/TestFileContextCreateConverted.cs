using FluentAssertions;
using Microsoft.AspNetCore.Http;
using SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Requests;
using SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Requests.Validators;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Extensions;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Systems.Contracts.FileContexts.Requests.Validators
{
    public class TestFileContextCreateConverted
    {
        [Fact]
        public void Validate_WithValidFormFile_ReturnValid()
        {
            // Arrange
            var formFile = new FormFile(null!, 0, 1, string.Empty, "FooName.test");

            var dto = new FileContextCreateConverted(formFile, 0);

            var validator = new FileContextCreateConvertedValidator();

            // Act
            var result = validator.Validate(dto);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_WithNullFormFile_ReturnInvalid()
        {
            // Arrange
            var dto = new FileContextCreateConverted(null!, 0);

            var validator = new FileContextCreateConvertedValidator();

            // Act
            var result = validator.Validate(dto);

            // Assert
            result.Should().NotBeValid()
                .And.HaveSingleError("The file must be provided for the request.");
        }

        [Fact]
        public void Validate_WithZeroFormFileLength_ReturnInvalid()
        {
            // Arrange
            var formFile = new FormFile(null!, 0, 0, string.Empty, "FooName.test");

            var dto = new FileContextCreateConverted(formFile, 0);

            var validator = new FileContextCreateConvertedValidator();

            // Act
            var result = validator.Validate(dto);

            // Assert
            result.Should().NotBeValid()
                .And.HaveSingleError("File size cannot be zero.");
        }

        [Fact]
        public void Validate_WithNullFileName_ReturnInvalid()
        {
            // Arrange
            var formFile = new FormFile(null!, 0, 1, string.Empty, null!);

            var dto = new FileContextCreateConverted(formFile, 0);

            var validator = new FileContextCreateConvertedValidator();

            // Act
            var result = validator.Validate(dto);

            // Assert
            result.Should().NotBeValid()
                .And.HaveSingleError("File name must not be empty.");
        }

        [Fact]
        public void Validate_WithEmptyName_ReturnInvalid()
        {
            // Arrange
            var formFile = new FormFile(null!, 0, 1, string.Empty, string.Empty);

            var dto = new FileContextCreateConverted(formFile, 0);

            var validator = new FileContextCreateConvertedValidator();

            // Act
            var result = validator.Validate(dto);

            // Assert
            result.Should().NotBeValid()
                .And.HaveSingleError("File name must not be empty.");
        }

        [Fact]
        public void Validate_WithWhiteSpaceFileName_ReturnInvalid()
        {
            // Arrange
            var formFile = new FormFile(null!, 0, 1, string.Empty, "     ");

            var dto = new FileContextCreateConverted(formFile, 0);

            var validator = new FileContextCreateConvertedValidator();

            // Act
            var result = validator.Validate(dto);

            // Assert
            result.Should().NotBeValid()
                .And.HaveSingleError("File name must not be empty.");
        }
    }
}
