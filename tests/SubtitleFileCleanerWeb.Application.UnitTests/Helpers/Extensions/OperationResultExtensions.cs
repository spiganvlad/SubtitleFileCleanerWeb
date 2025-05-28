using AwesomeAssertions.Execution;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.FluentObjects;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Helpers.Extensions;

public static class OperationResultExtensions
{
    public static OperationResultAssertion<T> Should<T>(this OperationResult<T> result)
    {
        return new OperationResultAssertion<T>(result, AssertionChain.GetOrCreate());
    }
}
