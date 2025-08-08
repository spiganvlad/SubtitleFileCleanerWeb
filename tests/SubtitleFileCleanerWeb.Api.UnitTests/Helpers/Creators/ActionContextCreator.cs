using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Creators;

public static class ActionContextCreator
{
    public static ActionContext Create(
        HttpContext? context = null,
        RouteData? routeData = null,
        ActionDescriptor? actionDescriptor = null,
        ModelStateDictionary? modelStateDictionary = null)
    {
        context ??= new DefaultHttpContext();
        routeData ??= new RouteData();
        actionDescriptor ??= new ActionDescriptor();
        modelStateDictionary ??= new ModelStateDictionary();

        return new ActionContext(context, routeData, actionDescriptor, modelStateDictionary);
    }
}
