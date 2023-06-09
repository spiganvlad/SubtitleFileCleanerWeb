using MediatR;
using Microsoft.EntityFrameworkCore;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContexts.Commands;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Domain.Exceptions;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Application.FileContexts.CommandHandlers;

public class UpdateFileContextNameHandler : IRequestHandler<UpdateFileContextName, OperationResult<FileContext>>
{
    private readonly ApplicationDbContext _ctx;

    public UpdateFileContextNameHandler(ApplicationDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<OperationResult<FileContext>> Handle(UpdateFileContextName request, CancellationToken cancellationToken)
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

            fileContext.UpdateName(request.FileName);

            _ctx.FileContexts.Update(fileContext);
            await _ctx.SaveChangesAsync(cancellationToken);

            result.Payload = fileContext;
        }
        catch (FileContextNotValidException ex)
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
