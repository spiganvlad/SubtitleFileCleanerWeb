using SubtitleFileCleanerWeb.Api.Controllers.V1;
using SubtitleFileCleanerWeb.Application.Models;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Wrappers
{
    public class BaseControllerWrapper : BaseController
    {
        public new IActionResult HandleErrorResponse(List<Error> errors)
        {
            return base.HandleErrorResponse(errors);
        }
    }
}
