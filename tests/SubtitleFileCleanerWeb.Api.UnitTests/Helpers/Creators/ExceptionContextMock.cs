﻿using Microsoft.AspNetCore.Mvc.Filters;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Creators;

public static class ExceptionContextMock
{
    public static Mock<ExceptionContext> Create()
    {
        return new Mock<ExceptionContext>(new object[]
        {
            TestActionContext.Create(),
            new List<IFilterMetadata>()
        });
    }
}
