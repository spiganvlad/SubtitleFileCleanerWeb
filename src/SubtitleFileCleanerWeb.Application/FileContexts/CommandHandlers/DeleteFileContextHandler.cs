using MediatR;
using Microsoft.EntityFrameworkCore;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.Commands;
using SubtitleFileCleanerWeb.Application.FileContexts.Commands;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Application.FileContexts.CommandHandlers;

public class DeleteFileContextHandler : IRequestHandler<DeleteFileContext, OperationResult<FileContext>>
{
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _ctx;

    public DeleteFileContextHandler(IMediator mediator, ApplicationDbContext ctx)
    {
        _mediator = mediator;
        _ctx = ctx;
    }

    public async Task<OperationResult<FileContext>> Handle(DeleteFileContext request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<FileContext>();

        try
        {
            var fileContext = await _ctx.FileContexts
                .FirstOrDefaultAsync(fc => fc.FileContextId == request.FileContextId, cancellationToken);

            if (fileContext == null)
            {
                result.AddError(ErrorCode.NotFound,
                    string.Format(FileContextErrorMessages.FileContextNotFound, request.FileContextId));
                return result;
            }

            var deleteContent = new DeleteFileContent(fileContext.FileContextId.ToString());
            var contentResult = await _mediator.Send(deleteContent, cancellationToken);
            if (contentResult.IsError)
            {
                contentResult.Errors.ForEach(e => result.AddError(e.Code, e.Message!));
                return result;
            }

            _ctx.FileContexts.Remove(fileContext);
            await _ctx.SaveChangesAsync(cancellationToken);

            result.Payload = fileContext;
        }
        catch (Exception ex)
        {
            result.AddUnknownError(ex.Message);
        }

        return result;
    }
}
