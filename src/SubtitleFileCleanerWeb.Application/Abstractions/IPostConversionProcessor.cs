using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;

namespace SubtitleFileCleanerWeb.Application.Abstractions;

public interface IPostConversionProcessor
{
    public Task<OperationResult<Stream>> ProcessAsync(Stream contentStream, ConversionType conversionType,
        CancellationToken cancellationToken, params PostConversionOption[] options);
}
