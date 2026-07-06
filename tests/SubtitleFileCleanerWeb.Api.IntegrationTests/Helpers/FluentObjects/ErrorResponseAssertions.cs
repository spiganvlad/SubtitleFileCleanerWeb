using AwesomeAssertions.Execution;
using AwesomeAssertions.Extensions;
using AwesomeAssertions.Primitives;
using SubtitleFileCleanerWeb.Api.Contracts.Common;

namespace SubtitleFileCleanerWeb.Api.IntegrationTests.Helpers.FluentObjects;

public class ErrorResponseAssertions : ObjectAssertions<ErrorResponse, ErrorResponseAssertions>
{
    public ErrorResponseAssertions(ErrorResponse response, AssertionChain assertionChain)
        : base(response, assertionChain) { }

    public AndConstraint<ErrorResponseAssertions> HaveStatusCode(int statusCode)
    {
        Subject.StatusCode.Should().Be(statusCode);

        return new AndConstraint<ErrorResponseAssertions>(this);
    }

    public AndConstraint<ErrorResponseAssertions> HaveStatusPhrase(string statusPhrase)
    {
        Subject.StatusPhrase.Should().Be(statusPhrase);

        return new AndConstraint<ErrorResponseAssertions>(this);
    }

    public AndConstraint<ErrorResponseAssertions> HaveSingleError(string errorMessage)
    {
        Subject.Errors.Should().ContainSingle()
            .Which.Should().Be(errorMessage);

        return new AndConstraint<ErrorResponseAssertions>(this);
    }

    public AndConstraint<ErrorResponseAssertions> HaveTimestampCloseToUtcNow()
    {
        Subject.Timestamp.Should().BeCloseTo(DateTime.UtcNow, 1.Seconds());

        return new AndConstraint<ErrorResponseAssertions>(this);
    }
}
