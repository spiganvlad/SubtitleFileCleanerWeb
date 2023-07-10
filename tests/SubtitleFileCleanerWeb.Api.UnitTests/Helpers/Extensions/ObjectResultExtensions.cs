using Microsoft.AspNetCore.Mvc;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.FluentObjects;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Extensions;

public static class ObjectResultExtensions
{
    public static ObjectResultAssertion Should(this ObjectResult result)
    {
        return new ObjectResultAssertion(result);
    }
}
