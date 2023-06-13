using FluentAssertions.Primitives;
using SubtitleFileCleanerWeb.Application.Models;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Helpers.FluentOperationResult;

public class OperationResultAssertion<T> : ObjectAssertions<OperationResult<T>, OperationResultAssertion<T>>
{
    public OperationResultAssertion(OperationResult<T> result) : base(result) { }
}
