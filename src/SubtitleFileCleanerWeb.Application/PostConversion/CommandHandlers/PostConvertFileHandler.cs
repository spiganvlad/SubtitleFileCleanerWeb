using MediatR;
using SubtitleFileCleanerWeb.Application.Abstractions;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.PostConversion.Commands;

namespace SubtitleFileCleanerWeb.Application.PostConversion.CommandHandlers;

public class PostConvertFileHandler : IRequestHandler<PostConvertFile, OperationResult<Stream>>
{
    private readonly IPostConversionProcessor _processor;

    public PostConvertFileHandler(IPostConversionProcessor processor)
    {
        _processor = processor;
    }

    public async Task<OperationResult<Stream>> Handle(PostConvertFile request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<Stream>();

        try
        {
            var postConversionResult = await _processor.ProcessAsync(request.ContentStream, request.ConversionType,
                cancellationToken, request.ConversionOptions);
            if (postConversionResult.IsError)
            {
                postConversionResult.Errors.ForEach(e => result.AddError(e.Code, e.Message!));
                return result;
            }

            result.Payload = postConversionResult.Payload;
        }
        catch (Exception ex)
        {
            result.AddUnknownError(ex.Message);
        }

        return result;
    }
}
