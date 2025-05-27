using AwesomeAssertions.Execution;
using FluentValidation.Results;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.FluentObjects;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Extensions;

public static class ValidationResultExtensions
{
    public static ValidationResultAssertion Should(this ValidationResult result)
    {
        return new ValidationResultAssertion(result, AssertionChain.GetOrCreate());
    }
}
