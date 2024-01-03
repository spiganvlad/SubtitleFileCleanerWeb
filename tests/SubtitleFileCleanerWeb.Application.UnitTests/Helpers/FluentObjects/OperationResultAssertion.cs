using FluentAssertions;
using FluentAssertions.Primitives;
using SubtitleFileCleanerWeb.Application.Enums;
using SubtitleFileCleanerWeb.Application.Models;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Helpers.FluentObjects;

public class OperationResultAssertion<T> : ObjectAssertions<OperationResult<T>, OperationResultAssertion<T>>
{
    private readonly AndConstraint<OperationResultAssertion<T>> _andConstraintThis;

    public OperationResultAssertion(OperationResult<T> result) : base(result)
    {
        _andConstraintThis = new AndConstraint<OperationResultAssertion<T>>(this);
    }

    #region Payload
    public AndConstraint<OperationResultAssertion<T>> HaveDefaultPayload()
    {
        var isPayloadDefault = CheckIfPayloadDefault();

        Assert.True(isPayloadDefault, "Expected Payload to be default value, but found not default value.");
        
        return _andConstraintThis;
    }

    public AndWhichConstraint<OperationResultAssertion<T>, T> HaveNotDefaultPayload()
    {
        var isPayloadDefault = CheckIfPayloadDefault();

        Assert.False(isPayloadDefault, "Expected Payload not to be default value, but found default value.");

        return new AndWhichConstraint<OperationResultAssertion<T>, T>(this, Subject.Payload!);
    }

    private bool CheckIfPayloadDefault()
    {
        return EqualityComparer<T>.Default.Equals(Subject.Payload, default);
    }
    #endregion
    #region State
    public AndConstraint<OperationResultAssertion<T>> NotBeInErrorState()
    {
        Subject.IsError.Should().BeFalse();

        return _andConstraintThis;
    }

    public AndConstraint<OperationResultAssertion<T>> BeInErrorState()
    {
        Subject.IsError.Should().BeTrue();

        return _andConstraintThis;
    }
    #endregion
    #region Errors
    public AndConstraint<OperationResultAssertion<T>> HaveNoErrors()
    {
        Subject.IsError.Should().BeFalse();
        Subject.Errors.Should().BeEmpty();

        return _andConstraintThis;
    }

    public AndConstraint<OperationResultAssertion<T>> HaveSingleError(ErrorCode errorCode, string errorMessage)
    {
        Subject.Errors.Should().ContainSingle();
        Subject.Errors[0].Code.Should().Be(errorCode);
        Subject.Errors[0].Message.Should().Be(errorMessage);

        return _andConstraintThis;
    }

    public AndConstraint<OperationResultAssertion<T>> HaveMultipleErrors(params Error[] errors)
    {
        Subject.Errors.Should().BeEquivalentTo(errors, options => options.WithStrictOrdering());

        return _andConstraintThis;
    }
    #endregion
}
