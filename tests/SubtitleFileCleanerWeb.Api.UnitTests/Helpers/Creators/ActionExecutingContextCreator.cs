using Microsoft.AspNetCore.Mvc.Filters;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Creators
{
    public static class ActionExecutingContextCreator
    {
        public static ActionExecutingContext Create(
            ActionContext? actionContext = null,
            List<IFilterMetadata>? filters = null,
            Dictionary<string, object?>? actionArguments = null,
            object controller = null!)
        {
            actionContext ??= ActionContextCreator.Create();
            filters ??= [];
            actionArguments ??= [];

            return new ActionExecutingContext(
            
                actionContext,
                filters,
                actionArguments,
                controller
            );
        }
    }
}
