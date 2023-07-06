using FluentAssertions;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.Extensions;
using SubtitleFileCleanerWeb.Application.UnitTests.Helpers.FluentObjects;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Helpers.Extensions;

public static class OperationResultExtensions
{
    public static OperationResultAssertion<T> Should<T>(this OperationResult<T> result)
    {
        return new OperationResultAssertion<T>(result);
    }

    public static AndConstraint<OperationResultAssertion<T>> ContainSingleError<T>(this OperationResultAssertion<T> assertion,
        ErrorCode errorCode, string errorMessage)
    {
        assertion.Subject.IsError.Should().BeTrue();
        assertion.Subject.Errors.Should().ContainSingle();
        assertion.Subject.Errors[0].Code.Should().Be(errorCode);
        assertion.Subject.Errors[0].Message.Should().Be(errorMessage);

        return new AndConstraint<OperationResultAssertion<T>>(assertion);
    }

    public static AndConstraint<OperationResultAssertion<T>> ContainsNoErrors<T>(this OperationResultAssertion<T> assertion)
    {
        assertion.Subject.IsError.Should().BeFalse();
        assertion.Subject.Errors.Should().BeEmpty();

        return new AndConstraint<OperationResultAssertion<T>>(assertion);
    }
}
