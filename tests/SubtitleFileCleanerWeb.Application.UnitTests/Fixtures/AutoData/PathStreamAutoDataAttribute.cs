using AutoFixture;
using AutoFixture.Xunit3;
using SubtitleFileCleanerWeb.Application.UnitTests.Fixtures.Customizations;
using SubtitleFileCleanerWeb.Application.UnitTests.Fixtures.Specimens;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Fixtures.AutoData;

public class PathStreamAutoDataAttribute : AutoDataAttribute
{
    public PathStreamAutoDataAttribute() : base(() =>
    {
        var fixture = new Fixture();

        fixture.Customize(new ReadOnlyStreamCustomization());
        fixture.Customizations.Add(new PartialPathSpecimen());

        return fixture;
    }) { }
        
}
