using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;

namespace SubtitleFileCleanerWeb.Application.Abstractions;

public interface IPostConversionProcessor
{
    /// <summary>
    /// Asynchronously converts a stream based on the specified post conversion options
    /// </summary>
    /// <param name="contentStream">Stream to be converted</param>
    /// <param name="conversionType">Conversion type</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request</param>
    /// <param name="options">Options defining what actions will be performed on the stream</param>
    /// <returns">A task that represents the asynchronous conversion operation.
    /// The task result contains operation result with the converted stream as the payload</returns>
    public Task<OperationResult<Stream>> ProcessAsync(Stream contentStream, ConversionType conversionType,
        CancellationToken cancellationToken, params PostConversionOption[] options);
}
