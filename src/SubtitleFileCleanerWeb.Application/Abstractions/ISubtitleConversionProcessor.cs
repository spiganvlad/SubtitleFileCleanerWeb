using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;

namespace SubtitleFileCleanerWeb.Application.Abstractions;

public interface ISubtitleConversionProcessor
{
    /// <summary>
    /// Asynchronously converts a stream based on the specified conversion type
    /// </summary>
    /// <param name="contentStream">Stream to be converted</param>
    /// <param name="conversionType">Conversion type</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request</param>
    /// <returns">A task that represents the asynchronous conversion operation.
    /// The task result contains operation result with the converted stream as the payload</returns>
    public Task<OperationResult<Stream>> ProcessAsync(Stream contentStream, ConversionType conversionType, CancellationToken cancellationToken);
}
