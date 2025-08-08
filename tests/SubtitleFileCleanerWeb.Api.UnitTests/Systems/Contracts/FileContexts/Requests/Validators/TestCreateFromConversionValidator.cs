using SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Requests;
using SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Requests.Validators;
using SubtitleFileCleanerWeb.Api.Options;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Systems.Contracts.FileContexts.Requests.Validators;

public class TestCreateFromConversionValidator
{
    private readonly long _maxFileLength;
    private readonly CreateFromConversionValidator _sut;

    public TestCreateFromConversionValidator()
    {
        _maxFileLength = 2;

        var formFileOptions = new FormFileOptions { MaxFileLength = _maxFileLength };

        var optionsMock = Substitute.For<IOptions<FormFileOptions>>();
        optionsMock.Value.Returns(formFileOptions);

        _sut = new(optionsMock);
    }

    [Fact]
    public void Validate_WithValidFormFile_ReturnValid()
    {
        // Arrange
        var formFile = new FormFile(null!, 0, 1, string.Empty, "FooName.test");

        var request = new CreateFromConversionRequest(formFile, 0);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Should().NotBeNull()
            .And.BeValid();
    }

    [Fact]
    public void Validate_WithNullFormFile_ReturnInvalid()
    {
        // Arrange
        var request = new CreateFromConversionRequest(null!, 0);

        // Act
        var result = _sut.Validate(request);

        // Assert
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

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Should().NotBeNull()
            .And.NotBeValid()
            .And.HaveSingleError("File size cannot be zero.");
    }

    [Fact]
    public void Validate_WithMoreThanAllowedFileLength_ReturnInvalid()
    {
        // Arrange
        var formFile = new FormFile(null!, 0, _maxFileLength, string.Empty, "FooName.test");

        var request = new CreateFromConversionRequest(formFile, 0);

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Should().NotBeNull()
            .And.NotBeValid()
            .And.HaveSingleError($"File size cannot be more then {_maxFileLength}.");
    }

    [Fact]
    public void Validate_WithNullFileName_ReturnInvalid()
    {
        // Arrange
        var formFile = new FormFile(null!, 0, 1, string.Empty, null!);

        var request = new CreateFromConversionRequest(formFile, 0);

        // Act
        var result = _sut.Validate(request);

        // Assert
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

        // Act
        var result = _sut.Validate(request);

        // Assert
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

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.Should().NotBeNull()
            .And.NotBeValid()
            .And.HaveSingleError("File name must not be empty.");
    }
}
