using Microsoft.AspNetCore.Mvc;
using SubtitleFileCleanerWeb.Api.Filters;
using SubtitleFileCleanerWeb.Application.FileContexts.Queries;

namespace SubtitleFileCleanerWeb.Api.Controllers.V1;

public class FileContentController : BaseController
{
    [HttpGet]
    [Route(ApiRoutes.Common.GuidIdRoute)]
    [ValidateGuid("guidId")]
    public async Task<IActionResult> DownloadContent(string guidId, CancellationToken cancellationToken)
    {
        var request = new GetFileContextWithContentById(Guid.Parse(guidId));

        var result = await Mediator.Send(request, cancellationToken);
        if (result.IsError)
            return HandleErrorResponse(result.Errors);

        var fs = new FileStreamResult(result.Payload!.FileContent!.Content, "application/octet-stream")
        { FileDownloadName = result.Payload.Name };

        return fs;
    }
}
