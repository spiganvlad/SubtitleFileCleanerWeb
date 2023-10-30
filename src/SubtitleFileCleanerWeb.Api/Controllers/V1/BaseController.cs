using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SubtitleFileCleanerWeb.Api.Contracts.Common;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;

namespace SubtitleFileCleanerWeb.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route(ApiRoutes.BaseRoute)]
public abstract class BaseController : ControllerBase
{
    private IMediator? _mediatorInstance;
    private IMapper? _mapper;

    protected IMediator Mediator => _mediatorInstance ??= (IMediator)HttpContext.RequestServices.GetService(typeof(IMediator))!;
    protected IMapper Mapper => _mapper ??= (IMapper)HttpContext.RequestServices.GetService(typeof(IMapper))!;

    protected IActionResult HandleErrorResponse(List<Error> errors)
    {
        var response = new ErrorResponse();

        if (errors.Any(e => e.Code == ErrorCode.NotFound))
        {
            var error = errors.FirstOrDefault(e => e.Code == ErrorCode.NotFound);

            response.StatusCode = 404;
            response.StatusPhrase = "Not Found";
            response.Timestamp = DateTime.UtcNow;
            response.Errors.Add(error!.Message!);

            return NotFound(response);
        }

        if (errors.Any(e => e.Code == ErrorCode.UnprocessableContent))
        {
            var error = errors.FirstOrDefault(e => e.Code == ErrorCode.UnprocessableContent);

            response.StatusCode = 422;
            response.StatusPhrase = "Unprocessable Content";
            response.Timestamp = DateTime.UtcNow;
            response.Errors.Add(error!.Message!);

            return UnprocessableEntity(response);
        }

        response.StatusCode = 400;
        response.StatusPhrase = "Bad Request";
        response.Timestamp = DateTime.UtcNow;
        errors.ForEach(e => response.Errors.Add(e!.Message!));

        return BadRequest(response);
    }
}
