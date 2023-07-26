using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;

namespace SubtitleFileCleanerWeb.Application.Abstractions;

public interface IPostConverter
{
    /// <summary>
    /// Specifies what type of post conversion operation it performs
    /// </summary>
    public PostConversionOption PostConversionOption { get; }

    /// <summary>
    /// Asynchronously converts a stream based on the <see cref="PostConversionOption"/> it defines
    /// </summary>
    /// <param name="contentStream">Stream to be converted</param>
    /// <param name="cancellationToken">The token to monitor for cancellation request</param>
    /// <returns>A task that represents the asynchronous conversion operation.
    /// The task result contains operation result with the converted stream as the payload</returns>
    public Task<OperationResult<Stream>> ConvertAsync(Stream contentStream, CancellationToken cancellationToken);
}
