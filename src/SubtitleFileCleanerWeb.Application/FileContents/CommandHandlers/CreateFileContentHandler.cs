using MediatR;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.Commands;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Domain.Exceptions;
using SubtitleFileCleanerWeb.Infrastructure.Blob;
using SubtitleFileCleanerWeb.Infrastructure.Exceptions;

namespace SubtitleFileCleanerWeb.Application.FileContents.CommandHandlers;

public class CreateFileContentHandler : IRequestHandler<CreateFileContent, OperationResult<FileContent>>
{
    private readonly IBlobStorageContext _blobContext;

    public CreateFileContentHandler(IBlobStorageContext blobContext)
    {
        _blobContext = blobContext;
    }

    public async Task<OperationResult<FileContent>> Handle(CreateFileContent request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<FileContent>();

        try
        {
            var fileContent = FileContent.Create(request.ContentStream);
            await _blobContext.CreateContentAsync(request.FileContextId.ToString(), fileContent.Content, cancellationToken);
            
            result.Payload = fileContent;
        }
        catch (FileContentNotValidException ex)
        {
            ex.ValidationErrors.ForEach(message => result.AddError(ErrorCode.ValidationError, message));
        }
        catch (BlobStorageOperationException ex)
        {
            result.AddError(ErrorCode.BlobContextOperationException, ex.Message);
        }
        catch (Exception ex)
        {
            result.AddUnknownError(ex.Message);
        }

        return result;
    }
}
