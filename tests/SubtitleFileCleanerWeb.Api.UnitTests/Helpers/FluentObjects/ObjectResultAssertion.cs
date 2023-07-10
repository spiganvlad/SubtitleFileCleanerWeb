using FluentAssertions;
using FluentAssertions.Primitives;
using Microsoft.AspNetCore.Mvc;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.FluentObjects;

public class ObjectResultAssertion : ObjectAssertions<ObjectResult, ObjectResultAssertion>
{
    private readonly AndConstraint<ObjectResultAssertion> _andConstraintThis;

    public ObjectResultAssertion(ObjectResult result): base(result)
    {
        _andConstraintThis = new AndConstraint<ObjectResultAssertion>(this);
    }

    public AndConstraint<ObjectResultAssertion> HaveStatusCode(int statusCode)
    {
        Subject.StatusCode = statusCode;

        return _andConstraintThis;
    }

    public AndConstraint<ObjectResultAssertion> HaveNotNullValue()
    {
        Subject.Value.Should().NotBeNull();

        return _andConstraintThis;
    }

    public AndWhichConstraint<ObjectResultAssertion, T> HaveValueOfType<T>()
    {
        var typedSubject = Subject.Value.Should().BeOfType<T>().Which;

        return new AndWhichConstraint<ObjectResultAssertion, T>(this, typedSubject);
    }
}
