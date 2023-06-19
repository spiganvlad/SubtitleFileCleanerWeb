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

    public static AndConstraint<ErrorResponseAssertion> HaveStatusCode(this ErrorResponseAssertion response, int statusCode)
    {
        response.Subject.StatusCode.Should().Be(statusCode);

        return new AndConstraint<ErrorResponseAssertion>(response);
    }

    public static AndConstraint<ErrorResponseAssertion> HaveStatusPhrase(this ErrorResponseAssertion response, string statusPhrase)
    {
        response.Subject.StatusPhrase.Should().Be(statusPhrase);

        return new AndConstraint<ErrorResponseAssertion>(response);
    }

    public static AndConstraint<ErrorResponseAssertion> HaveTimeStampCloseTo(this ErrorResponseAssertion response,
        DateTime nearbyTime, TimeSpan precision)
    {
        response.Subject.Timestamp.Should().BeCloseTo(nearbyTime, precision);

        return new AndConstraint<ErrorResponseAssertion>(response);
    }

    public static AndConstraint<ErrorResponseAssertion> HaveSingleError(this ErrorResponseAssertion response, string errorMessage)
    {
        response.Subject.Errors.Should().ContainSingle();
        response.Subject.Errors[0].Should().Be(errorMessage);

        return new AndConstraint<ErrorResponseAssertion>(response);
    }
}
