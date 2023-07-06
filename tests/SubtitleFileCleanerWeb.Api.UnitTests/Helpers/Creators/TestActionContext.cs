using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Creators;

public static class TestActionContext
{
    public static ActionContext Create()
    {
        return new ActionContext(new DefaultHttpContext(), new RouteData(),
            new ActionDescriptor(), new ModelStateDictionary());
    }
}
