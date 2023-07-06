using FluentAssertions;
using SubtitleFileCleanerWeb.Api.Contracts.Common;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Extensions;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.FluentObjects;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Extensions;

public static class ErrorResponseExtensions
{
    public static ErrorResponseAssertion Should(this ErrorResponse response)
    {
        return new ErrorResponseAssertion(response);
    }

    public static AndConstraint<ErrorResponseAssertion> HaveStatusCode(this ErrorResponseAssertion assertion, int statusCode)
    {
        assertion.Subject.StatusCode.Should().Be(statusCode);

        return new AndConstraint<ErrorResponseAssertion>(assertion);
    }

    public static AndConstraint<ErrorResponseAssertion> HaveStatusPhrase(this ErrorResponseAssertion assertion, string statusPhrase)
    {
        assertion.Subject.StatusPhrase.Should().Be(statusPhrase);

        return new AndConstraint<ErrorResponseAssertion>(assertion);
    }

    public static AndConstraint<ErrorResponseAssertion> HaveTimeStampCloseTo(this ErrorResponseAssertion assertion,
        DateTime nearbyTime, TimeSpan precision)
    {
        assertion.Subject.Timestamp.Should().BeCloseTo(nearbyTime, precision);

        return new AndConstraint<ErrorResponseAssertion>(assertion);
    }

    public static AndConstraint<ErrorResponseAssertion> HaveSingleError(this ErrorResponseAssertion assertion, string errorMessage)
    {
        assertion.Subject.Errors.Should().ContainSingle()
            .Which.Should().Be(errorMessage);

        return new AndConstraint<ErrorResponseAssertion>(assertion);
    }

    public static AndConstraint<ErrorResponseAssertion> HaveErrors(this ErrorResponseAssertion assertion, params string[] errorMessages)
    {
        assertion.Subject.Errors.Should().HaveCount(errorMessages.Length)
            .And.Contain(errorMessages);

        return new AndConstraint<ErrorResponseAssertion>(assertion);
    }
}
