using System.Reflection;

namespace SubtitleFileCleanerWeb.Application.UnitTests.Helpers.Reflection;

public static class InnerExceptionsCreator
{
    public static T Create<T>(string message) where T : Exception
    {
        var type = typeof(T);
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        object[] parameters = [message];

        return (T)Activator.CreateInstance(type, flags, null, parameters, null)!;
    }
}
