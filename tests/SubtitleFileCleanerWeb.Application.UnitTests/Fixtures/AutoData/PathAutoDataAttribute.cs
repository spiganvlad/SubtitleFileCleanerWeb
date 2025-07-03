using AutoFixture;
using AutoFixture.Xunit3;
using SubtitleFileCleanerWeb.Application.UnitTests.Fixtures.Specimens;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Fixtures.AutoData;

public class PathAutoDataAttribute : AutoDataAttribute
{
    public PathAutoDataAttribute() : base(() =>
    {
        var fixture = new Fixture();

        fixture.Customizations.Add(new PartialPathSpecimen());

        return fixture;
    }) { }
}
