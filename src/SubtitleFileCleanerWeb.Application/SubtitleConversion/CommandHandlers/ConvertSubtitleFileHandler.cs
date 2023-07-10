using MediatR;
using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.SubtitleConversion.Commands;

namespace SubtitleFileCleanerWeb.Application.SubtitleConversion.CommandHandlers;

public class ConvertSubtitleFileHandler : IRequestHandler<ConvertSubtitleFile, OperationResult<Stream>>
{
    private readonly ISubtitleConversionProcessor _conversionProcessor;

    public ConvertSubtitleFileHandler(ISubtitleConversionProcessor conversionProcessor)
    {
        _conversionProcessor = conversionProcessor;
    }

    public async Task<OperationResult<Stream>> Handle(ConvertSubtitleFile request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Stream>();

        try
        {
            var conversionResult = await _conversionProcessor.ProcessAsync(request.ContentStream, request.ConversionType, cancellationToken);
            if (conversionResult.IsError)
            {
                result.CopyErrors(conversionResult.Errors);
                return result;
            }

            result.Payload = conversionResult.Payload;
        }
        catch (Exception ex)
        {
            result.AddError(ErrorCode.UnknownError, ex.Message);
        }

        return result;
    }
}
