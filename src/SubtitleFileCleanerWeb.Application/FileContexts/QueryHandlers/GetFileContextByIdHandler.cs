using MediatR;
using Microsoft.EntityFrameworkCore;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContexts.Queries;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Application.FileContexts.QueryHandlers;

public class GetFileContextByIdHandler : IRequestHandler<GetFileContextById, OperationResult<FileContext>>
{
    private readonly ApplicationDbContext _ctx;

    public GetFileContextByIdHandler(ApplicationDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<OperationResult<FileContext>> Handle(GetFileContextById request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<FileContext>();

        try
        {
            var fileContext = await _ctx.FileContexts.
                FirstOrDefaultAsync(fc => fc.FileContextId == request.FileContextId, cancellationToken);

            if (fileContext == null)
            {
                result.AddError(ErrorCode.NotFound,
                    string.Format(FileContextErrorMessages.FileContextNotFound, request.FileContextId));
                return result;
            }

            result.Payload = fileContext;
        }
        catch (Exception ex)
        {
            result.AddUnknownError(ex.Message);
        }
        
        return result;
    }
}
