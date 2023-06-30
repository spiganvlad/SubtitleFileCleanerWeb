using MediatR;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContents.Commands;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Infrastructure.Blob;
using SubtitleFileCleanerWeb.Infrastructure.Exceptions;

namespace SubtitleFileCleanerWeb.Application.FileContents.CommandHandlers;

public class DeleteFileContentHandler : IRequestHandler<DeleteFileContent, OperationResult<bool>>
{
    private readonly IBlobStorageContext _blobStorageContext;

    public DeleteFileContentHandler(IBlobStorageContext blobStorageContext)
    {
        _blobStorageContext = blobStorageContext;
    }

    public async Task<OperationResult<bool>> Handle(DeleteFileContent request, CancellationToken cancellationToken)
    {
        var result = new OperationResult<bool>();

        try
        {
            await _blobStorageContext.DeleteContentAsync(request.Path, cancellationToken);

            result.Payload = true;
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
