using FluentAssertions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Helpers.FluentOperationResult;

public static class OperationResultExtensions
{
    public static OperationResultAssertion<T> Should<T>(this OperationResult<T> result)
    {
        return new OperationResultAssertion<T>(result);
    }

    public static AndConstraint<OperationResultAssertion<T>> ContainSingleError<T>(this OperationResultAssertion<T> result,
        ErrorCode errorCode, string errorMessage)
    {
        result.Subject.IsError.Should().BeTrue();
        result.Subject.Errors.Should().ContainSingle();
        result.Subject.Errors[0].Code.Should().Be(errorCode);
        result.Subject.Errors[0].Message.Should().Be(errorMessage);

        return new AndConstraint<OperationResultAssertion<T>>(result);
    }

    public static AndConstraint<OperationResultAssertion<T>> ContainsNoErrors<T>(this OperationResultAssertion<T> result)
    {
        result.Subject.IsError.Should().BeFalse();
        result.Subject.Errors.Should().BeEmpty();

        return new AndConstraint<OperationResultAssertion<T>>(result);
    }
}
