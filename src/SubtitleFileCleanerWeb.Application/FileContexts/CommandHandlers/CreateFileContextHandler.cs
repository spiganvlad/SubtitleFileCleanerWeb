using MediatR;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.Commands;
using SubtitleFileCleanerWeb.Application.FileContexts.Commands;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Domain.Exceptions.FileContextAggregateExceptions;
using SubtitleFileCleanerWeb.Infrastructure.Persistence;

namespace SubtitleFileCleanerWeb.Application.FileContexts.CommandHandlers;

public class CreateFileContextHandler : IRequestHandler<CreateFileContext, OperationResult<FileContext>>
{
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _ctx;

    public CreateFileContextHandler(IMediator mediator, ApplicationDbContext ctx)
    {
        _mediator = mediator;
        _ctx = ctx;
    }

    public async Task<OperationResult<FileContext>> Handle(CreateFileContext request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<FileContext>();

        try
        {
            var fileContextName = Path.ChangeExtension(request.FileName, ".txt");
            var fileContext = FileContext.Create(fileContextName, request.ContentStream.Length);

            var fileContentPath = Path.Combine("Unauthorized", fileContext.FileContextId.ToString());
            var createFileContent = new CreateFileContent(fileContentPath, request.ContentStream);

            var fileContentResult = await _mediator.Send(createFileContent, cancellationToken);
            if (fileContentResult.IsError)
            {
                result.CopyErrors(fileContentResult.Errors);
                return result;
            }
            fileContext.SetContent(fileContentResult.Payload!);

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
