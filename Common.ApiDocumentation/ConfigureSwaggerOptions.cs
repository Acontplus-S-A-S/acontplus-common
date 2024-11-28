using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Common.ApiDocumentation;
public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    : IConfigureNamedOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));
        }
    }

    public void Configure(string? name, SwaggerGenOptions options)
    {
        Configure(options);
    }
    
    private static OpenApiInfo CreateVersionInfo(ApiVersionDescription description)
    {
        var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name ?? "My API";

        var info = new OpenApiInfo
        {
            Title = assemblyName,
            Version = description.ApiVersion.ToString(),
            Description = description.IsDeprecated
                ? "This API version has been deprecated. Please use one of the new APIs available from the explorer."
                : "This is the API documentation for the application.",
            Contact = new OpenApiContact
            {
                Name = "Support Team",
                Email = "zaratec@acontplus.com.ec",
                Url = new Uri("https://getapp.acontplus.com")
            },
            License = new OpenApiLicense
            {
                Name = "MIT License",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        };

        return info;
    }
}
