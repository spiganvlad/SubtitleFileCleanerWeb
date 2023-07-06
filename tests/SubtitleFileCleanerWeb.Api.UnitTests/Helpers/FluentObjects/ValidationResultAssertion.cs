using FluentAssertions.Primitives;
using FluentValidation.Results;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.FluentObjects;

public class ValidationResultAssertion : ObjectAssertions<ValidationResult, ValidationResultAssertion>
{
    public ValidationResultAssertion(ValidationResult result) : base(result) { }
}
