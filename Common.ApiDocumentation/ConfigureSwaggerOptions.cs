using System.Reflection;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Common.ApiDocumentation;
public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IConfiguration configuration)
    : IConfigureNamedOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateVersionInfo(description, configuration));
        }
    }

    public void Configure(string? name, SwaggerGenOptions options)
    {
        Configure(options);
    }
    
    private static OpenApiInfo CreateVersionInfo(ApiVersionDescription description, IConfiguration configuration)
    {
        var swaggerInfo = configuration.GetSection("SwaggerInfo");

        return new OpenApiInfo
        {
            Title = Assembly.GetEntryAssembly()?.GetName().Name ?? "My API",
            Version = description.ApiVersion.ToString(),
            Description = description.IsDeprecated
                ? "This API version has been deprecated. Please use one of the new APIs available from this application."
                : "Visit the API documentation for the application.",
            Contact = new OpenApiContact
            {
                Name = swaggerInfo["ContactName"] ?? "Default Support Team",
                Email = swaggerInfo["ContactEmail"] ?? "default@support.com",
                Url = new Uri(swaggerInfo["ContactUrl"] ?? "https://default.support.com")
            },
            License = new OpenApiLicense
            {
                Name = swaggerInfo["LicenseName"] ?? "MIT License",
                Url = new Uri(swaggerInfo["LicenseUrl"] ?? "https://opensource.org/licenses/MIT")
            }
        };
    }
}
