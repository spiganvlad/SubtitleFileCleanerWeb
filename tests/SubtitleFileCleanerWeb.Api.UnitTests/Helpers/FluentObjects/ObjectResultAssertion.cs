using AwesomeAssertions.Execution;
using AwesomeAssertions.Primitives;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.FluentObjects;

public class ObjectResultAssertion : ObjectAssertions<ObjectResult, ObjectResultAssertion>
{
    private readonly AndConstraint<ObjectResultAssertion> _andConstraintThis;

    public ObjectResultAssertion(ObjectResult result, AssertionChain assertionChain) : base(result, assertionChain)
    {
        _andConstraintThis = new AndConstraint<ObjectResultAssertion>(this);
    }

    public AndConstraint<ObjectResultAssertion> HaveStatusCode(int statusCode)
    {
        Subject.StatusCode.Should().Be(statusCode);

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
