using System.Reflection;
using SubtitleFileCleanerWeb.Api.Registrars;
using SubtitleFileCleanerWeb.Api.Registrars.Pipelines;
using SubtitleFileCleanerWeb.Api.Registrars.Services;

namespace SubtitleFileCleanerWeb.Api.Extensions;

public static class RegisterExtensions
{
    public static void RegisterServices(this WebApplicationBuilder builder, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var registers = GetRegisters<IServicesRegister>(assembly);

            foreach (var register in registers)
            {
                register.Register(builder);
            }
        }
    }

    public static void RegisterPipelines(this WebApplication app, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var registers = GetRegisters<IPipelineRegister>(assembly);

            foreach (var register in registers)
            {
                register.Register(app);
            }
        }
    }

    private static IEnumerable<T> GetRegisters<T>(Assembly assembly) where T :IRegister
    {
        return assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(T)) && !t.IsAbstract && !t.IsInterface)
            .Select(Activator.CreateInstance)
            .Cast<T>();
    }
}
