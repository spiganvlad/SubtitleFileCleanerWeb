using SubtitleFileCleanerWeb.Application.Enums;

namespace SubtitleFileCleanerWeb.Application.Abstractions;

public interface ISubtitleConverter
{
    public ConversionType ConversionType { get; }

    /// <summary>
    /// Asynchronously converts the subtitle stream, thereby extracting everything that is not related to subtitle formatting
    /// </summary>
    /// <param name="contentStream">Stream to be converted</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request</param>
    /// <returns">A task that represents the asynchronous conversion operation.</returns>
    public Task<Stream> ConvertAsync(Stream contentStream, CancellationToken cancellationToken);
}
