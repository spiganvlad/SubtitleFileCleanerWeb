using SubtitleFileCleanerWeb.Api.Contracts.Common;
using SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Responses;
using SubtitleFileCleanerWeb.Api.IntegrationTests.Factories;
using SubtitleFileCleanerWeb.Api.IntegrationTests.Helpers.Blob;
using SubtitleFileCleanerWeb.Api.IntegrationTests.Helpers.Database;
using SubtitleFileCleanerWeb.Api.IntegrationTests.Helpers.Extensions;
using SubtitleFileCleanerWeb.Api.IntegrationTests.Helpers.TestSubtitles;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Api.IntegrationTests.Systems.Controllers.V1;

public class TestFileContextController : IDisposable
{
    private readonly SqliteDatabaseInitializer _sqliteDatabaseInitializer = new();
    private readonly FileSystemBlobInitializer _fileSystemBlobInitializer = new();
    private readonly SubtitleFileCleanerWebApplicationFactory _applicationFactory;
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    public TestFileContextController()
    {
        _applicationFactory = new(
            _sqliteDatabaseInitializer,
            _fileSystemBlobInitializer);
    }

    [Theory, AutoData]
    public async Task GetById_WithExistingContext_ReturnFileContextResponse
        (FileContext fileContext)
    {
        // Arrange
        _sqliteDatabaseInitializer.AddSeedData(fileContext);

        var route = $"api/FileContext/{fileContext.FileContextId}";

        var client = _applicationFactory.CreateClient();

        // Act
        var response = await client.GetAsync(route, _cancellationToken);

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.OK)
            .And.HaveReasonPhrase("OK")
            .And.HaveContentType("application/json", "utf-8")
            .And.HaveNotNullBody<FileContextResponse>()
            
            .Which.Should().HaveId(fileContext.FileContextId)
            .And.HaveName(fileContext.Name)
            .And.HaveSize(fileContext.ContentSize);
    }

    [Theory, AutoData]
    public async Task GetById_WithNonExistingContext_ReturnErrorResponse
        (Guid guidId)
    {
        // Arrange
        var route = $"api/FileContext/{guidId}";

        var client = _applicationFactory.CreateClient();

        // Act
        var response = await client.GetAsync(route, _cancellationToken);

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.NotFound)
            .And.HaveReasonPhrase("Not Found")
            .And.HaveContentType("application/json", "utf-8")
            .And.HaveNotNullBody<ErrorResponse>()
            
            .Which.Should().HaveStatusCode(404)
            .And.HaveStatusPhrase("Not Found")
            .And.HaveSingleError($"No file context found with id: { guidId }.")
            .And.HaveTimestampCloseToUtcNow();
    }

    [Theory, AutoData]
    public async Task GetById_WithInvalidGuid_ReturnErrorResponse
        (Guid guidId)
    {
        // Arrange
        var invalidGuid = guidId.ToInvalidString();
        var route = $"api/FileContext/{invalidGuid}";

        var client = _applicationFactory.CreateClient();

        // Act
        var response = await client.GetAsync(route, _cancellationToken);

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.BadRequest)
            .And.HaveReasonPhrase("Bad Request")
            .And.HaveContentType("application/json", "utf-8")
            .And.HaveNotNullBody<ErrorResponse>()

            .Which.Should().HaveStatusCode(400)
            .And.HaveStatusPhrase("Bad Request")
            .And.HaveSingleError($"Invalid GUID format of the parameter: guidId.")
            .And.HaveTimestampCloseToUtcNow();
    }

    [Theory, AutoData]
    public async Task CreateFromConversion_WithSrtContentAndDeleteTagsOption_ReturnFileContextResponse
        (string fileName)
    {
        // Arrange
        var route = $"api/FileContext/{ConversionType.Srt}";

        var multipartContent = new MultipartFormDataContent();

        var fileContent = new ByteArrayContent(SrtTestSubtitle.SrtByteArrayContent);
        multipartContent.Add(fileContent, "File", fileName + ".srt");

        var optionsContent = new StringContent(PostConversionOption.DeleteBasicTags.ToString());
        multipartContent.Add(optionsContent, "PostConversionOptions");

        var client = _applicationFactory.CreateClient();

        // Act
        var response = await client.PostAsync(route, multipartContent, _cancellationToken);

        // Assert
        var body = response.Should().HaveStatusCode(HttpStatusCode.Created)
            .And.HaveReasonPhrase("Created")
            .And.HaveContentType("application/json", "utf-8")
            .And.HaveNotNullBody<FileContextResponse>()
            .Which;

        body.Should().HaveName(fileName + ".txt")
            .And.HaveSize(SrtTestSubtitle.ResultWithoutTags.ConvertedContentLength);

        var savedContext = await DatabaseHelper.GetSavedContextAsync(body.Id, _applicationFactory.Services, _cancellationToken);
        savedContext.Should().NotBeNull();

        var savedContent = await BlobHelper.GetSavedContentAsync(body.Id, _applicationFactory.Services, _cancellationToken);
        savedContent.Should().NotBeNull();
    }

    [Theory, AutoData]
    public async Task CreateFromConversion_WithSrtContentAndZeroPostConversionOptions_ReturnFileContextResponse
        (string fileName)
    {
        // Arrange
        var route = $"api/FileContext/{ConversionType.Srt}";

        var multipartContent = new MultipartFormDataContent();

        var fileContent = new ByteArrayContent(SrtTestSubtitle.SrtByteArrayContent);
        multipartContent.Add(fileContent, "File", fileName + ".srt");

        var client = _applicationFactory.CreateClient();

        // Act
        var response = await client.PostAsync(route, multipartContent, _cancellationToken);

        // Assert
        var body = response.Should().HaveStatusCode(HttpStatusCode.Created)
            .And.HaveReasonPhrase("Created")
            .And.HaveContentType("application/json", "utf-8")
            .And.HaveNotNullBody<FileContextResponse>()
            .Which;

        body.Should().HaveName(fileName + ".txt")
            .And.HaveSize(SrtTestSubtitle.ResultWithTags.ConvertedContentLength);

        var savedContext = await DatabaseHelper.GetSavedContextAsync(body.Id, _applicationFactory.Services, _cancellationToken);
        savedContext.Should().NotBeNull();

        var savedContent = await BlobHelper.GetSavedContentAsync(body.Id, _applicationFactory.Services, _cancellationToken);
        savedContent.Should().NotBeNull();
    }

    [Theory, AutoData]
    public async Task CreateFromConversion_WithNotSubtitleContent_ReturnErrorResponse
        (string fileName)
    {
        // Arrange
        byte[] content = [1, 2, 3, 4, 5];
        var route = $"api/FileContext/{ConversionType.Srt}";

        var multipartContent = new MultipartFormDataContent();

        var fileContent = new ByteArrayContent(content);
        multipartContent.Add(fileContent, "File", fileName + ".srt");

        var client = _applicationFactory.CreateClient();

        // Act
        var response = await client.PostAsync(route, multipartContent, _cancellationToken);

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity)
            .And.HaveReasonPhrase("Unprocessable Entity")
            .And.HaveContentType("application/json", "utf-8")
            .And.HaveNotNullBody<ErrorResponse>()

            .Which.Should().HaveStatusCode(422)
            .And.HaveStatusPhrase("Unprocessable Content")
            .And.HaveSingleError($"Converted content produced nothing.")
            .And.HaveTimestampCloseToUtcNow();
    }

    [Theory, AutoData]
    public async Task UpdateNamen_WithExistingFileContext_ReturnFileContextResponse
        (FileContext fileContext, string nameToUpdate)
    {
        // Arrange
        _sqliteDatabaseInitializer.AddSeedData(fileContext);

        var route = $"api/FileContext/{fileContext.FileContextId}";

        var httpContent = JsonContent.Create(new { Name = nameToUpdate });

        var client = _applicationFactory.CreateClient();

        // Act
        var response = await client.PatchAsync(route, httpContent, _cancellationToken);

        // Assert
        var body = response.Should().HaveStatusCode(HttpStatusCode.OK)
            .And.HaveReasonPhrase("OK")
            .And.HaveContentType("application/json", "utf-8")
            .And.HaveNotNullBody<FileContextResponse>().Which;

        body.Should().HaveId(fileContext.FileContextId)
            .And.HaveName(nameToUpdate)
            .And.HaveSize(fileContext.ContentSize);

        var savedContext = await DatabaseHelper.GetSavedContextAsync(body.Id, _applicationFactory.Services, _cancellationToken);
        savedContext.Should().NotBeNull();
        savedContext.Name.Should().Be(nameToUpdate);
    }

    [Theory, AutoData]
    public async Task UpdateNamen_WithNonExistingFileContext_ReturnErrorResponse
        (Guid guidId, string nameToUpdate)
    {
        // Arrange
        var route = $"api/FileContext/{guidId}";

        var httpContent = JsonContent.Create(new { Name = nameToUpdate });

        var client = _applicationFactory.CreateClient();

        // Act
        var response = await client.PatchAsync(route, httpContent, _cancellationToken);

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.NotFound)
            .And.HaveReasonPhrase("Not Found")
            .And.HaveContentType("application/json", "utf-8")
            .And.HaveNotNullBody<ErrorResponse>()

            .Which.Should().HaveStatusCode(404)
            .And.HaveStatusPhrase("Not Found")
            .And.HaveSingleError($"No file context found with id: { guidId }.")
            .And.HaveTimestampCloseToUtcNow();
    }

    [Theory, AutoData]
    public async Task UpdateNamen_WithInvalidGuid_ReturnErrorResponse
        (Guid guidId, string nameToUpdate)
    {
        // Arrange
        var invalidGuid = guidId.ToInvalidString();
        var route = $"api/FileContext/{invalidGuid}";

        var httpContent = JsonContent.Create(new { Name = nameToUpdate });

        var client = _applicationFactory.CreateClient();

        // Act
        var response = await client.PatchAsync(route, httpContent, _cancellationToken);

        // Arrange
        response.Should().HaveStatusCode(HttpStatusCode.BadRequest)
            .And.HaveReasonPhrase("Bad Request")
            .And.HaveContentType("application/json", "utf-8")
            .And.HaveNotNullBody<ErrorResponse>()

            .Which.Should().HaveStatusCode(400)
            .And.HaveStatusPhrase("Bad Request")
            .And.HaveSingleError($"Invalid GUID format of the parameter: guidId.")
            .And.HaveTimestampCloseToUtcNow();
    }

    [Theory, AutoData]
    public async Task UpdateNamen_WithEmptyName_ReturnErrorResponse
        (FileContext fileContext)
    {
        // Arrange
        _sqliteDatabaseInitializer.AddSeedData(fileContext);

        var route = $"api/FileContext/{fileContext.FileContextId}";

        var httpContent = JsonContent.Create(new { Name = string.Empty });

        var client = _applicationFactory.CreateClient();

        // Act
        var response = await client.PatchAsync(route, httpContent, _cancellationToken);

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.BadRequest)
            .And.HaveReasonPhrase("Bad Request")
            .And.HaveContentType("application/json", "utf-8")
            .And.HaveNotNullBody<ErrorResponse>()

            .Which.Should().HaveStatusCode(400)
            .And.HaveStatusPhrase("Bad Request")
            .And.HaveSingleError($"Update name must not be empty.")
            .And.HaveTimestampCloseToUtcNow();
    }

    [Theory, AutoData]
    public async Task Delete_WithExistingFileContext_ReturnFileContextResponse
        (FileContext fileContext, byte[] content)
    {
        // Arrange
        _sqliteDatabaseInitializer.AddSeedData(fileContext);
        _fileSystemBlobInitializer.AddSeedData(fileContext.FileContextId, content);

        var route = $"api/FileContext/{fileContext.FileContextId}";

        var client = _applicationFactory.CreateClient();

        // Act
        var response = await client.DeleteAsync(route, _cancellationToken);

        // Assert
        var body = response.Should().HaveStatusCode(HttpStatusCode.OK)
            .And.HaveReasonPhrase("OK")
            .And.HaveContentType("application/json", "utf-8")
            .And.HaveNotNullBody<FileContextResponse>()
            .Which;

        body.Should().HaveId(fileContext.FileContextId)
            .And.HaveName(fileContext.Name)
            .And.HaveSize(fileContext.ContentSize);

        var deletedContext = await DatabaseHelper.GetSavedContextAsync(fileContext.FileContextId, _applicationFactory.Services, _cancellationToken);
        deletedContext.Should().BeNull();

        var deletedContent = await BlobHelper.GetSavedContentAsync(fileContext.FileContextId, _applicationFactory.Services, _cancellationToken);
        deletedContent.Should().BeNull();
    }

    [Theory, AutoData]
    public async Task Delete_WithNonExistingFileContext_ReturnErrorResponse
        (FileContext fileContext, byte[] content)
    {
        // Arrange
        _fileSystemBlobInitializer.AddSeedData(fileContext.FileContextId, content);

        var route = $"api/FileContext/{fileContext.FileContextId}";

        var client = _applicationFactory.CreateClient();

        // Act
        var response = await client.DeleteAsync(route, _cancellationToken);

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.NotFound)
            .And.HaveReasonPhrase("Not Found")
            .And.HaveContentType("application/json", "utf-8")
            .And.HaveNotNullBody<ErrorResponse>()

            .Which.Should().HaveStatusCode(404)
            .And.HaveStatusPhrase("Not Found")
            .And.HaveSingleError($"No file context found with id: { fileContext.FileContextId }.")
            .And.HaveTimestampCloseToUtcNow();
    }

    [Theory, AutoData]
    public async Task Delete_WithNonExistingFileContent_ReturnErrorResponse
        (FileContext fileContext)
    {
        // Arrange
        _sqliteDatabaseInitializer.AddSeedData(fileContext);

        var route = $"api/FileContext/{fileContext.FileContextId}";

        var client = _applicationFactory.CreateClient();

        // Act
        var response = await client.DeleteAsync(route, _cancellationToken);

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.BadRequest)
            .And.HaveReasonPhrase("Bad Request")
            .And.HaveContentType("application/json", "utf-8")
            .And.HaveNotNullBody<ErrorResponse>()

            .Which.Should().HaveStatusCode(400)
            .And.HaveStatusPhrase("Bad Request")
            .And.HaveTimestampCloseToUtcNow();
    }

    [Theory, AutoData]
    public async Task Delete_WithInvalidGuid_ReturnErrorResponse
        (Guid guidId)
    {
        // Arrange
        var invalidGuid = guidId.ToInvalidString();

        var route = $"api/FileContext/{invalidGuid}";

        var client = _applicationFactory.CreateClient();

        // Act
        var response = await client.DeleteAsync(route, _cancellationToken);

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.BadRequest)
            .And.HaveReasonPhrase("Bad Request")
            .And.HaveContentType("application/json", "utf-8")
            .And.HaveNotNullBody<ErrorResponse>()

            .Which.Should().HaveStatusCode(400)
            .And.HaveStatusPhrase("Bad Request")
            .And.HaveSingleError($"Invalid GUID format of the parameter: guidId.")
            .And.HaveTimestampCloseToUtcNow();
    }

    public void Dispose()
    {
        _applicationFactory.Dispose();
    }
}
