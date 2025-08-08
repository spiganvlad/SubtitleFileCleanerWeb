using AutoFixture;
using AutoFixture.Xunit3;
using SubtitleFileCleanerWeb.Application.UnitTests.Fixtures.Customizations;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Fixtures.AutoData;

public class StreamAutoDataAttribute : AutoDataAttribute
{
    public StreamAutoDataAttribute() : base(() =>
        new Fixture().Customize(new ReadOnlyStreamCustomization())) { }
}
