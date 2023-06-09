using MediatR;
using Microsoft.EntityFrameworkCore;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContexts.Commands;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Application.FileContexts.CommandHandlers;

public class DeleteFileContextHandler : IRequestHandler<DeleteFileContext, OperationResult<FileContext>>
{
    private readonly ApplicationDbContext _ctx;

    public DeleteFileContextHandler(ApplicationDbContext ctx)
    {
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
