using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using FluentValidation.Results;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.FluentObjects;

public class ValidationResultAssertion : ObjectAssertions<ValidationResult, ValidationResultAssertion>
{
    private readonly AndConstraint<ValidationResultAssertion> _andConstraintThis;

    public ValidationResultAssertion(ValidationResult result, AssertionChain assertionChain) : base(result, assertionChain)
    {
        _andConstraintThis = new AndConstraint<ValidationResultAssertion>(this);
    }

    public AndConstraint<ValidationResultAssertion> BeValid()
    {
        Subject.IsValid.Should().BeTrue();

        return _andConstraintThis;
    }

    public AndConstraint<ValidationResultAssertion> NotBeValid()
    {
        Subject.IsValid.Should().BeFalse();

        return _andConstraintThis;
    }

    public AndConstraint<ValidationResultAssertion> HaveSingleError(string errorMessage)
    {
        Subject.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(errorMessage);

        return _andConstraintThis;
    }
}
