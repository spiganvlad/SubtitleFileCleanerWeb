using MediatR;
using Microsoft.EntityFrameworkCore;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.Queries;
using SubtitleFileCleanerWeb.Application.FileContexts.Queries;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Domain.Exceptions;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Application.FileContexts.QueryHandlers;

public class GetFileContextWithContentByIdHandler : IRequestHandler<GetFileContextWithContentById, OperationResult<FileContext>>
{
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _ctx;

    public GetFileContextWithContentByIdHandler(IMediator mediator, ApplicationDbContext ctx)
    {
        _mediator = mediator;
        _ctx = ctx;
    }

    public async Task<OperationResult<FileContext>> Handle(GetFileContextWithContentById request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<FileContext>();

        try
        {
            var fileContext = await _ctx.FileContexts.FirstOrDefaultAsync(f => f.FileContextId == request.FileContextId, cancellationToken);
            if (fileContext == null)
            {
                result.AddError(ErrorCode.NotFound, string.Format(FileContextErrorMessages.FileContextNotFound,
                    request.FileContextId));
                return result;
            }

            var getFileContent = new GetFileContentById(fileContext.FileContextId.ToString());
            var fileContentResult = await _mediator.Send(getFileContent, cancellationToken);
            if (fileContentResult.IsError)
            {
                result.CopyErrors(fileContentResult.Errors);
                return result;
            }

            fileContext.SetContent(fileContentResult.Payload!);
            result.Payload = fileContext;
        }
        catch (FileContentNotValidException ex)
        {
            ex.ValidationErrors.ForEach(message => result.AddError(ErrorCode.ValidationError, message));
        }
        catch (Exception ex)
        {
            result.AddUnknownError(ex.Message);
        }

        return result;
    }
}
