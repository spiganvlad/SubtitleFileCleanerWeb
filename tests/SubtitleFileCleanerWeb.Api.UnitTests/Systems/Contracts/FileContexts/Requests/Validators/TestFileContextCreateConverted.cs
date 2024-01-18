using SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Requests;
using SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Requests.Validators;
using SubtitleFileCleanerWeb.Api.Options;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Systems.Contracts.FileContexts.Requests.Validators
{
    public class TestFileContextCreateConverted
    {
        private readonly Mock<IOptions<FormFileOptions>> _optionsMock;
        private readonly FormFileOptions _formFileOptions;

        public TestFileContextCreateConverted()
        { 
            _optionsMock = new();

            _formFileOptions = new FormFileOptions { MaxFileLength = 2 };
            _optionsMock.SetupGet(o => o.Value).Returns(_formFileOptions);
        }

        [Fact]
        public void Validate_WithValidFormFile_ReturnValid()
        {
            // Arrange
            var formFile = new FormFile(null!, 0, 1, string.Empty, "FooName.test");

            var request = new CreateFromConversionRequest(formFile, 0);

            var validator = new CreateFromConversionValidator(_optionsMock.Object);

            // Act
            var result = validator.Validate(request);

            // Assert
            _optionsMock.VerifyGet(o => o.Value, Times.Exactly(2));

            result.Should().NotBeNull()
                .And.BeValid();
        }

        [Fact]
        public void Validate_WithNullFormFile_ReturnInvalid()
        {
            // Arrange
            var request = new CreateFromConversionRequest(null!, 0);

            var validator = new CreateFromConversionValidator(_optionsMock.Object);

            // Act
            var result = validator.Validate(request);

            // Assert
            _optionsMock.VerifyGet(o => o.Value, Times.Exactly(2));

            result.Should().NotBeNull()
                .And.NotBeValid()
                .And.HaveSingleError("The file must be provided for the request.");
        }

        [Fact]
        public void Validate_WithZeroFormFileLength_ReturnInvalid()
        {
            // Arrange
            var formFile = new FormFile(null!, 0, 0, string.Empty, "FooName.test");

            var request = new CreateFromConversionRequest(formFile, 0);

            var validator = new CreateFromConversionValidator(_optionsMock.Object);

            // Act
            var result = validator.Validate(request);

            // Assert
            _optionsMock.VerifyGet(o => o.Value, Times.Exactly(2));

            result.Should().NotBeNull()
                .And.NotBeValid()
                .And.HaveSingleError("File size cannot be zero.");
        }

        [Fact]
        public void Validate_WithMoreThanAllowedFileLength_ReturnInvalid()
        {
            // Arrange
            var formFile = new FormFile(null!, 0, _formFileOptions.MaxFileLength, string.Empty, "FooName.test");

            var request = new CreateFromConversionRequest(formFile, 0);

            var validator = new CreateFromConversionValidator(_optionsMock.Object);

            // Act
            var result = validator.Validate(request);

            // Assert
            _optionsMock.VerifyGet(o => o.Value, Times.Exactly(2));

            result.Should().NotBeNull()
                .And.NotBeValid()
                .And.HaveSingleError($"File size cannot be more then {_formFileOptions.MaxFileLength}.");
        }

        [Fact]
        public void Validate_WithNullFileName_ReturnInvalid()
        {
            // Arrange
            var formFile = new FormFile(null!, 0, 1, string.Empty, null!);

            var request = new CreateFromConversionRequest(formFile, 0);

            var validator = new CreateFromConversionValidator(_optionsMock.Object);

            // Act
            var result = validator.Validate(request);

            // Assert
            _optionsMock.VerifyGet(o => o.Value, Times.Exactly(2));

            result.Should().NotBeNull()
                .And.NotBeValid()
                .And.HaveSingleError("File name must not be empty.");
        }

        [Fact]
        public void Validate_WithEmptyName_ReturnInvalid()
        {
            // Arrange
            var formFile = new FormFile(null!, 0, 1, string.Empty, string.Empty);

            var request = new CreateFromConversionRequest(formFile, 0);

            var validator = new CreateFromConversionValidator(_optionsMock.Object);

            // Act
            var result = validator.Validate(request);

            // Assert
            _optionsMock.VerifyGet(o => o.Value, Times.Exactly(2));

            result.Should().NotBeNull()
                .And.NotBeValid()
                .And.HaveSingleError("File name must not be empty.");
        }

        [Fact]
        public void Validate_WithWhiteSpaceFileName_ReturnInvalid()
        {
            // Arrange
            var formFile = new FormFile(null!, 0, 1, string.Empty, "     ");

            var request = new CreateFromConversionRequest(formFile, 0);

            var validator = new CreateFromConversionValidator(_optionsMock.Object);

            // Act
            var result = validator.Validate(request);

            // Assert
            _optionsMock.VerifyGet(o => o.Value, Times.Exactly(2));

            result.Should().NotBeNull()
                .And.NotBeValid()
                .And.HaveSingleError("File name must not be empty.");
        }
    }
}
