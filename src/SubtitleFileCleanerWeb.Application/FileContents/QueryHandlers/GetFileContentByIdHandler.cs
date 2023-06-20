using MediatR;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.Queries;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;
using SubtitleFileCleanerWeb.Domain.Exceptions;
using SubtitleFileCleanerWeb.Infrastructure.Blob;

namespace SubtitleFileCleanerWeb.Application.FileContents.QueryHandlers;

public class GetFileContentByIdHandler : IRequestHandler<GetFileContentById, OperationResult<FileContent>>
{
    private readonly IBlobStorageContext _blobContext;

    public GetFileContentByIdHandler(IBlobStorageContext blobContext)
    {
        _blobContext = blobContext;
    }

    public async Task<OperationResult<FileContent>> Handle(GetFileContentById request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<FileContent>();

        try
        {
            var contentStream = await _blobContext.GetContentStreamAsync(request.FileContextId.ToString(), cancellationToken);
            if (contentStream == null)
            {
                result.AddError(ErrorCode.NotFound,
                    string.Format(FileContentErrorMessages.FileContentNotFound, request.FileContextId));
                return result;
            }

            var fileContent = FileContent.Create(contentStream);
            result.Payload = fileContent;
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
