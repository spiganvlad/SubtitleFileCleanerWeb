using FluentAssertions.Primitives;
using SubtitleFileCleanerWeb.Api.Contracts.Common;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.FluentObjects;

public class ErrorResponseAssertion : ObjectAssertions<ErrorResponse, ErrorResponseAssertion>
{
    public ErrorResponseAssertion(ErrorResponse response) : base(response) { }
}
