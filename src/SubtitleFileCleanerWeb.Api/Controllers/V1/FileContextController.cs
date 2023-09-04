using Microsoft.AspNetCore.Mvc;
using SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Requests;
using SubtitleFileCleanerWeb.Api.Contracts.FileContexts.Responses;
using SubtitleFileCleanerWeb.Api.Filters;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.FileContexts.Commands;
using SubtitleFileCleanerWeb.Application.FileContexts.Queries;
using SubtitleFileCleanerWeb.Application.PostConversion.Commands;
using SubtitleFileCleanerWeb.Application.SubtitleConversion.Commands;
using SubtitleFileCleanerWeb.Domain.Aggregates.FileContextAggregate;

namespace SubtitleFileCleanerWeb.Api.Controllers.V1;

[Route(ApiRoutes.BaseRoute)]
public class FileContextController : BaseController
{
    [HttpGet]
    [Route(ApiRoutes.Common.GuidIdRoute)]
    [ValidateGuid("guidId")]
    public async Task<IActionResult> GetById(string guidId, CancellationToken cancellationToken)
    {
        var request = new GetFileContextById(Guid.Parse(guidId));

        var result = await Mediator.Send(request, cancellationToken);
        if (result.IsError)
            return HandleErrorResponse(result.Errors);
            
        var response = Mapper.Map<FileContext, FileContextResponse>(result.Payload!);
        return Ok(response);
    }

    [HttpPost]
    [Route(ApiRoutes.FileContext.ConversionType)]
    [ValidateModel]
    public async Task<IActionResult> CreateFromConversion(ConversionType conversionType, CreateFromConversionRequest request, CancellationToken cancellationToken)
    {
        var contentStream = request.File.OpenReadStream();

        // Sending a stream to the conversion process
        var conversionRequest = new ConvertSubtitleFile(contentStream, conversionType);
        var conversionResult = await Mediator.Send(conversionRequest, cancellationToken);
        if (conversionResult.IsError)
            return HandleErrorResponse(conversionResult.Errors);

        contentStream = conversionResult.Payload!;

        // Sending a stream to the post conversion process if any post conversion options are specified
        if (request.PostConversionOptions is not null && request.PostConversionOptions.Length != 0)
        {
            var postConversionRequest = new PostConvertFile(contentStream, request.PostConversionOptions);
            var postConversionResult = await Mediator.Send(postConversionRequest, cancellationToken);
            if (postConversionResult.IsError)
                return HandleErrorResponse(postConversionResult.Errors);

            contentStream = postConversionResult.Payload!;
        }

        // Sending a request to create a file context
        var createContextRequest = new CreateFileContext(request.File.FileName, contentStream);
        var fileContextResult = await Mediator.Send(createContextRequest, cancellationToken);
        if (fileContextResult.IsError)
            return HandleErrorResponse(fileContextResult.Errors);

        var response = Mapper.Map<FileContext, FileContextResponse>(fileContextResult.Payload!);
        return Ok(response);
    }

    [HttpPatch]
    [Route(ApiRoutes.Common.GuidIdRoute)]
    [ValidateGuid("guidId")]
    [ValidateModel]
    public async Task<IActionResult> UpdateName(string guidId, [FromBody] UpdateNameRequest request, CancellationToken cancellationToken)
    {
        var updateRequest = new UpdateFileContextName(Guid.Parse(guidId), request.Name);

        var result = await Mediator.Send(updateRequest, cancellationToken);
        if (result.IsError)
            return HandleErrorResponse(result.Errors);

        var response = Mapper.Map<FileContext, FileContextResponse>(result.Payload!);
        return Ok(response);
    }

    [HttpDelete]
    [Route(ApiRoutes.Common.GuidIdRoute)]
    [ValidateGuid("guidId")]
    public async Task<IActionResult> Delete(string guidId, CancellationToken cancellationToken)
    {
        var request = new DeleteFileContext(Guid.Parse(guidId));

        var result = await Mediator.Send(request, cancellationToken);
        if (result.IsError)
            return HandleErrorResponse(result.Errors);

        var response = Mapper.Map<FileContext, FileContextResponse>(result.Payload!);
        return Ok(response);
    }
}
