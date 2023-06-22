using SubtitleFileCleanerWeb.Api.Extensions;

namespace SubtitleFileCleanerWeb.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.RegisterServices(typeof(Program).Assembly);

        var app = builder.Build();
        app.RegisterPipelines(typeof(Program).Assembly);

        app.Run();
    }
}