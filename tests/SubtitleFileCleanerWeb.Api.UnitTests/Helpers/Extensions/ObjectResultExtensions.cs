using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Extensions;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.FluentObjects;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Extensions;

public static class ObjectResultExtensions
{
    public static ObjectResultAssertion Should(this ObjectResult result)
    {
        return new ObjectResultAssertion(result);
    }

    public static AndConstraint<ObjectResultAssertion> HaveStatusCode(this ObjectResultAssertion assertion, int statusCode)
    {
        assertion.Subject.StatusCode = statusCode;

        return new AndConstraint<ObjectResultAssertion>(assertion);
    }

    public static AndConstraint<ObjectResultAssertion> HaveNotNullValue(this ObjectResultAssertion assertion)
    {
        assertion.Subject.Value.Should().NotBeNull();

        return new AndConstraint<ObjectResultAssertion>(assertion);
    }

    public static AndWhichConstraint<ObjectResultAssertion, T> HaveValueOfType<T>(this ObjectResultAssertion assertion)
    {
        var typedSubject = assertion.Subject.Value.Should().BeOfType<T>().Which;

        return new AndWhichConstraint<ObjectResultAssertion, T>(assertion, typedSubject);
    }
}
