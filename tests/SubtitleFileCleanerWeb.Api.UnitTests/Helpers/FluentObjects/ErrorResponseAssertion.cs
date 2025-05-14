using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using SubtitleFileCleanerWeb.Api.Contracts.Common;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.FluentObjects;

public class ErrorResponseAssertion : ObjectAssertions<ErrorResponse, ErrorResponseAssertion>
{
    private readonly AndConstraint<ErrorResponseAssertion> _andConstraintThis;

    public ErrorResponseAssertion(ErrorResponse response, AssertionChain assertionChain) : base(response, assertionChain)
    {
        _andConstraintThis = new AndConstraint<ErrorResponseAssertion>(this);
    }

    public AndConstraint<ErrorResponseAssertion> HaveStatusCode(int statusCode)
    {
        Subject.StatusCode.Should().Be(statusCode);

        return _andConstraintThis;
    }

    public AndConstraint<ErrorResponseAssertion> HaveStatusPhrase(string statusPhrase)
    {
        Subject.StatusPhrase.Should().Be(statusPhrase);

        return _andConstraintThis;
    }

    public AndConstraint<ErrorResponseAssertion> HaveTimeStampCloseTo(DateTime nearbyTime, TimeSpan precision)
    {
        Subject.Timestamp.Should().BeCloseTo(nearbyTime, precision);

        return _andConstraintThis;
    }

    public AndConstraint<ErrorResponseAssertion> HaveSingleError(string errorMessage)
    {
        Subject.Errors.Should().ContainSingle()
            .Which.Should().Be(errorMessage);

        return _andConstraintThis;
    }

    public AndConstraint<ErrorResponseAssertion> HaveErrors(params string[] errorMessages)
    {
        Subject.Errors.Should().HaveCount(errorMessages.Length)
            .And.Contain(errorMessages);

        return _andConstraintThis;
    }
}
