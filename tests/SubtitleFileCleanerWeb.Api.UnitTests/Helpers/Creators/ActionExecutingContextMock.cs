using Microsoft.AspNetCore.Mvc.Filters;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Creators;

public class ActionExecutingContextMock
{
    public static Mock<ActionExecutingContext> Create(
        ActionContext? actionContext = null,
        List<IFilterMetadata>? filters = null,
        Dictionary<string, object?>? actionArguments = null,
        object controller = null!
        )
    {
        actionContext ??= TestActionContext.Create();
        filters ??= [];
        actionArguments ??= [];

        return new Mock<ActionExecutingContext>(
        [
            actionContext,
            filters,
            actionArguments,
            controller
        ]);
    }
}
