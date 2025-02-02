using Microsoft.Extensions.Configuration;

namespace Services.Common.Configuration;

public static class AppConfiguration
{
    public static IConfiguration Load()
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        var sharedFolder = Environment.GetEnvironmentVariable("SHARED_SETTINGS_PATH");

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();

        // Load shared settings if the path is set
        if (!string.IsNullOrEmpty(sharedFolder))
        {
            var sharedFile = Path.Combine(sharedFolder, $"sharedsettings.{environment}.json");
            builder.AddJsonFile(sharedFile, optional: true, reloadOnChange: true);
        }

        return builder.Build();
    }
}
