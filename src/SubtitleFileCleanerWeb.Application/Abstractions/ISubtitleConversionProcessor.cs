using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;

namespace SubtitleFileCleanerWeb.Application.Abstractions;

public interface ISubtitleConversionProcessor
{
    public Task<OperationResult<Stream>> ProcessAsync(Stream contentStream, ConversionType conversionType, CancellationToken cancellationToken);
}
