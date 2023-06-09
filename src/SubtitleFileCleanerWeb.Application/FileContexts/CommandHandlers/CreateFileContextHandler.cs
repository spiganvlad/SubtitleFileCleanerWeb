using MediatR;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContexts.Commands;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Domain.Exceptions;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Application.FileContexts.CommandHandlers;

public class CreateFileContextHandler : IRequestHandler<CreateFileContext, OperationResult<FileContext>>
{
    private readonly ApplicationDbContext _ctx;

    public CreateFileContextHandler(ApplicationDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<OperationResult<FileContext>> Handle(CreateFileContext request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<FileContext>();

        try
        {
            var fileContext = FileContext.Create(request.FileName);

            _ctx.FileContexts.Add(fileContext);
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
