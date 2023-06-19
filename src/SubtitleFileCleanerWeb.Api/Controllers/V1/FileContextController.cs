using Microsoft.AspNetCore.Mvc;
using SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Responses;
using SubtitleFileCleanerWeb.Application.FileContexts.Commands;
using SubtitleFileCleanerWeb.Application.FileContexts.Queries;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Api.Controllers.V1;

[Route(ApiRoutes.BaseRoute)]
public class FileContextController : BaseController
{
    [HttpGet]
    [Route(ApiRoutes.Common.IdRoute)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var request = new GetFileContextById(id);

        var result = await Mediator.Send(request, cancellationToken);

        if (result.IsError)
            return HandleErrorResponse(result.Errors);
            
        var response = Mapper.Map<FileContext, FileContextResponse>(result.Payload!);
        return Ok(response);
    }

    [HttpPatch]
    [Route(ApiRoutes.Common.IdRoute)]
    public async Task<IActionResult> UpdateName(Guid id, string name, CancellationToken cancellationToken)
    {
        var request = new UpdateFileContextName(id, name);

        var result = await Mediator.Send(request, cancellationToken);

        if(result.IsError)
            return HandleErrorResponse(result.Errors);

        var response = Mapper.Map<FileContext, FileContextResponse>(result.Payload!);
        return Ok(response);
    }
}
