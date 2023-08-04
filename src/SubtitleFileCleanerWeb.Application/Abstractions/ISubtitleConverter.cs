using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;

namespace SubtitleFileCleanerWeb.Application.Abstractions;

public interface ISubtitleConverter
{
    public ConversionType ConversionType { get; }

    /// <summary>
    /// Asynchronously converts the subtitle stream, thereby extracting everything that is not related to subtitle formatting
    /// </summary>
    /// <param name="contentStream">Stream to be converted</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request</param>
    /// <returns">A task that represents the asynchronous conversion operation.
    /// The task result contains operation result with the converted stream as the payload</returns>
    public Task<OperationResult<Stream>> ConvertAsync(Stream contentStream, CancellationToken cancellationToken);
}
