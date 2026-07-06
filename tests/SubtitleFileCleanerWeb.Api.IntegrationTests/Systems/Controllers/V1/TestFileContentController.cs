using SubtitleFileCleanerWeb.Api.Contracts.Common;
using SubtitleFileCleanerWeb.Api.IntegrationTests.Factories;
using SubtitleFileCleanerWeb.Api.IntegrationTests.Helpers.Blob;
using SubtitleFileCleanerWeb.Api.IntegrationTests.Helpers.Database;
using SubtitleFileCleanerWeb.Api.IntegrationTests.Helpers.Extensions;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Api.IntegrationTests.Systems.Controllers.V1;

public class TestFileContentController : IDisposable
{
    private readonly SqliteDatabaseInitializer _sqliteDatabaseInitializer = new();
    private readonly FileSystemBlobInitializer _fileSystemBlobInitializer = new();
    private readonly SubtitleFileCleanerWebApplicationFactory _applicationFactory;
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;
    
    public TestFileContentController()
    {
        _applicationFactory = new(
            _sqliteDatabaseInitializer,
            _fileSystemBlobInitializer);
    }

    [Theory, AutoData]
    public async Task DownloadContent_WithExistingContext_ReturnByteArray
        (FileContext fileContext, byte[] content)
    {
        // Arrange
        _sqliteDatabaseInitializer.AddSeedData(fileContext);
        _fileSystemBlobInitializer.AddSeedData(fileContext.FileContextId, content);

        var route = $"api/FileContent/{fileContext.FileContextId}";

        var client = _applicationFactory.CreateClient();

        // Act
        var response = await client.GetAsync(route, _cancellationToken);

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.OK)
            .And.HaveReasonPhrase("OK")
            .And.HaveContentType("application/octet-stream")
            .And.HaveBodyAsByteArray()
            
            .Which.Should().BeEquivalentTo(content);
    }

    [Theory, AutoData]
    public async Task DownloadContent_WithNonExistingContext_ReturnErrorResponse
        (Guid guidId)
    {
        // Arrange
        var route = $"api/FileContent/{guidId}";

        var client = _applicationFactory.CreateClient();

        // Act
        var response = await client.GetAsync(route, _cancellationToken);

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.NotFound)
            .And.HaveReasonPhrase("Not Found")
            .And.HaveContentType("application/json")
            .And.HaveNotNullBody<ErrorResponse>()

            .Which.Should().HaveStatusCode(404)
            .And.HaveStatusPhrase("Not Found")
            .And.HaveSingleError($"No file context found with id: {guidId}.")
            .And.HaveTimestampCloseToUtcNow();
    }

    [Theory, AutoData]
    public async Task DownloadContent_WithInvalidGuid_ReturnErrorResponse
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

    public void Dispose()
    {
        _applicationFactory.Dispose();
    }
}
