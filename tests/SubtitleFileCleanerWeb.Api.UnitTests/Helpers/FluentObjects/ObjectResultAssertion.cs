using FluentAssertions.Primitives;
using Microsoft.AspNetCore.Mvc;

namespace SubtitleFileCleanerWeb.Api.UnitTests.Helpers.FluentObjects;

public class ObjectResultAssertion : ObjectAssertions<ObjectResult, ObjectResultAssertion>
{
    public ObjectResultAssertion(ObjectResult result): base(result) { }
}
