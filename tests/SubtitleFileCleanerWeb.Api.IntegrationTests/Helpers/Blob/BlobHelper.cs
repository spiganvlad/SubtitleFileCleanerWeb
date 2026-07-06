using Microsoft.Extensions.DependencyInjection;
using SubtitleFileCleanerWeb.Infrastructure.Blob;

namespace SubtitleFileCleanerWeb.Api.IntegrationTests.Helpers.Blob;

public static class BlobHelper
{
    public static async Task<string?> GetSavedContentAsync(Guid guidId, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        var blobContext = scope.ServiceProvider.GetRequiredService<IBlobStorageContext>();

        var path = Path.Combine("Unauthorized", guidId.ToString());

        using var savedContentStream = await blobContext.GetContentStreamAsync(path, cancellationToken);

        return savedContentStream is null
            ? null
            : await new StreamReader(savedContentStream).ReadToEndAsync(cancellationToken);
    }
}
