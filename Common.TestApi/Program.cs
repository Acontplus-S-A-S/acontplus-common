using System;
using Common.ApiDocumentation;
using Common.FactElect.Interfaces.Services;
using Common.FactElect.Services.External;
using Common.Logging;
using Common.Notifications.Abstractions;
using Common.Notifications.Services;
using Common.Services.Middleware;
using Common.TestApi.Data;
using Common.TestApi.Extensions;
using Microsoft.EntityFrameworkCore;
using Scrutor;
using Serilog;

// 1. Optional: Create a bootstrap logger for early startup issues
//    This captures logs from WebApplication.CreateBuilder() itself.
Log.Logger = new LoggerConfiguration()
           .WriteTo.Console()
           .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Get environment name once for clarity and re-use
    var environment = builder.Environment.EnvironmentName;

    // 2. Configure Serilog for the web host using the new extension method
    builder.Host.UseSerilog((hostContext, services, loggerConfiguration) =>
    {
        // Call the new method to apply your advanced logging settings to the LoggerConfiguration
        loggerConfiguration.ConfigureAdvancedLogger(hostContext.Configuration, environment);

        // This part tells Serilog to load its configuration from appsettings.json
        // and resolve any services (e.g., custom enrichers requiring DI)
        loggerConfiguration.ReadFrom.Configuration(hostContext.Configuration);
        loggerConfiguration.ReadFrom.Services(services);
    });

    // 3. Register your LoggingOptions class into the DI container
    //    This is where builder.Services (an IServiceCollection) is available.
    builder.Services.AddAdvancedLoggingOptions(builder.Configuration);

    // --- Start new try-catch block for service registration ---
    try
    {
        builder.Services.AddApplicationServices(builder.Configuration); // <--- This is a likely suspect
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        string[] nameSpaces =
        [
            "Common.Infrastructure.Repository.Implementations",
            "Common.Reports.Services",
            "Common.TestApi.Services",
            "Common.TestApi.Repositories.Implementations",
            "Common.Core.Security.Services"
        ];

        builder.Services.Scan(scan => scan
            .FromApplicationDependencies()
            .AddClasses(classes => classes.InNamespaces(nameSpaces))
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsImplementedInterfaces()
            .WithTransientLifetime()
        );

        builder.Services.AddTransient<IWebServiceSri, WebServiceSri>();
        builder.Services.AddTransient<ICustomerService, CustomerService>();
        builder.Services.AddTransient<IMailKitService, AmazonSesService>();
        builder.Services.AddDataProtection();
        builder.Services.AddSwaggerDocumentation();
        builder.Services.AddVersioningAndSwagger();

        builder.Services.AddDbContextPool<TestContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    }
    catch (Exception serviceEx)
    {
        Log.Fatal(serviceEx, "An error occurred during service registration.");
        // Re-throw or exit to ensure the host aborts, but now you have the log
        throw;
    }
    // --- End new try-catch block ---
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    // Use Serilog request logging BEFORE other middleware like UseRouting, UseAuthentication, etc.
    app.UseSerilogRequestLogging(); // Captures HTTP request/response details

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();

        app.UseSwaggerAndVersioning();
    }

    app.UseMiddleware<ExceptionMiddleware>();

    app.UseRouting();

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();

}
catch (Exception ex)
{
    // Catch any critical startup errors
    Log.Fatal(ex, "API host terminated unexpectedly.");
}
finally
{
    // Ensure all buffered logs are flushed on application shutdown
    Log.CloseAndFlush();
}
