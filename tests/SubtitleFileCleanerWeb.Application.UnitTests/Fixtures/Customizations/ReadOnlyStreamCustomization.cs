using AutoFixture;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Fixtures.Customizations;

public class ReadOnlyStreamCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Register<Stream>(() => new MemoryStream(fixture.Create<byte[]>(), false));
    }
}
