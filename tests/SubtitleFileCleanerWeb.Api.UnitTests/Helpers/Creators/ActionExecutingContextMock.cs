using Microsoft.AspNetCore.Mvc.Filters;
using Moq;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Creators;

public class ActionExecutingContextMock
{
    public static Mock<ActionExecutingContext> Create()
    {
        return new Mock<ActionExecutingContext>(new object[]
        {
            TestActionContext.Create(),
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            null!
        });
    }
}
