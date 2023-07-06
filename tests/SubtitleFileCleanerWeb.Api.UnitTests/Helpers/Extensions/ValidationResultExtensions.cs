using FluentAssertions;
using FluentValidation.Results;
using SubtitleFileCleanerWeb.Api.UnitTests.Helpers.FluentObjects;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.Extensions;

public static class ValidationResultExtensions
{
    public static ValidationResultAssertion Should(this ValidationResult result)
    {
        return new ValidationResultAssertion(result);
    }

    public static AndConstraint<ValidationResultAssertion> NotBeValid(this ValidationResultAssertion assertion)
    {
        assertion.Subject.IsValid.Should().BeFalse();

        return new AndConstraint<ValidationResultAssertion>(assertion);
    }

    public static AndConstraint<ValidationResultAssertion> HaveSingleError(this ValidationResultAssertion assertion, string errorMessage)
    {
        assertion.Subject.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be(errorMessage);

        return new AndConstraint<ValidationResultAssertion>(assertion);
    }
}
