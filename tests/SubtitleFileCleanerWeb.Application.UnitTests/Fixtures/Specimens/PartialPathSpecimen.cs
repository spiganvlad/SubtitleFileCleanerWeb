using System.Reflection;
using AutoFixture;
using AutoFixture.Kernel;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Fixtures.Specimens;

public class PartialPathSpecimen : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo { Name: "Path", PropertyType: Type t } && t == typeof(string))
        {
            return Path.Combine(context.Create<string>(), context.Create<string>());
        }

        return new NoSpecimen();
    }
}
