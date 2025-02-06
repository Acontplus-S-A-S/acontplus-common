using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Compact;
using Serilog.Sinks.MSSqlServer;

namespace Common.Logging;

public static class SerilogExtensions
{
    public static IServiceCollection AddAdvancedLogging(this IServiceCollection services, IConfiguration configuration)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                              Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
                              "Production";

        // Enable Serilog self-logging for debugging
        Serilog.Debugging.SelfLog.Enable(Console.WriteLine);

        // Read logging options from configuration
        var loggingOptions = new LoggingOptions();
        configuration.GetSection("AdvancedLogging").Bind(loggingOptions);

        // Parse the rolling interval
        var rollingIntervalString = loggingOptions.RollingInterval ?? "Day";
        var rollingInterval = (RollingInterval)Enum.Parse(typeof(RollingInterval), rollingIntervalString, true);

        // Set default values if retainedFileCountLimit or fileSizeLimitBytes are null or 0
        var retainedFileCountLimit = (loggingOptions.RetainedFileCountLimit ?? 7) == 0 ? 7 : loggingOptions.RetainedFileCountLimit.Value;
        var fileSizeLimitBytes = (loggingOptions.FileSizeLimitBytes ?? 10 * 1024 * 1024) == 0 ? 10 * 1024 * 1024 : loggingOptions.FileSizeLimitBytes.Value;

        // Create logger configuration
        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(loggingOptions.MinimumLogLevel)
            .Enrich.WithEnvironmentUserName()
            .Enrich.FromLogContext();

        // Add asynchronous file sink if enabled
        if (loggingOptions.EnableLocalFile && !string.IsNullOrEmpty(loggingOptions.LocalFilePath))
        {
            if (environment == "Production")
            {
                loggerConfiguration.WriteTo.Async(a => a.File(
                formatter: new CompactJsonFormatter(),
                path: loggingOptions.LocalFilePath,
                rollingInterval: rollingInterval,
                retainedFileCountLimit: loggingOptions.RetainedFileCountLimit,
                fileSizeLimitBytes: loggingOptions.FileSizeLimitBytes,
                encoding: System.Text.Encoding.UTF8,
                buffered: true,
                shared: false
            ));
            }
            else
            {
                loggerConfiguration.WriteTo.Console()
                    .WriteTo.Async(a => a.File(
                    path: loggingOptions.LocalFilePath,
                    rollingInterval: rollingInterval,
                    retainedFileCountLimit: loggingOptions.RetainedFileCountLimit,
                    encoding: System.Text.Encoding.UTF8,
                    buffered: true,
                    shared: false,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                ));
            }
        }

        // Add asynchronous S3 sink if enabled
        if (loggingOptions.EnableS3Logging &&
            !string.IsNullOrEmpty(loggingOptions.S3BucketName) &&
            !string.IsNullOrEmpty(loggingOptions.S3AccessKey) &&
            !string.IsNullOrEmpty(loggingOptions.S3SecretKey))
        {
            loggerConfiguration.WriteTo.Async(a => a.AmazonS3(
                path: "log.text",
                bucketName: loggingOptions.S3BucketName,
                Amazon.RegionEndpoint.USEast1,
                awsAccessKeyId: loggingOptions.S3AccessKey,
                awsSecretAccessKey: loggingOptions.S3SecretKey,
                encoding: System.Text.Encoding.UTF8,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: Serilog.Sinks.AmazonS3.RollingInterval.Minute,
                failureCallback: e => Console.WriteLine($"An error occurred in the S3 sink: {e.Message}")
            ));
        }

        // Add asynchronous SQL Server sink if enabled
        if (loggingOptions.EnableDatabaseLogging &&
            !string.IsNullOrEmpty(loggingOptions.DatabaseConnectionString))
        {
            var sinkOpts = new MSSqlServerSinkOptions
            {
                TableName = "Logs",
                SchemaName = "Common",
                AutoCreateSqlTable = true,
                BatchPostingLimit = 1000, // Batch logs to reduce database round-trips
                BatchPeriod = TimeSpan.FromSeconds(5) // Send logs every 5 seconds
            };

            var columnOpts = new ColumnOptions
            {
                Id = { DataType = SqlDbType.BigInt },
                LogEvent = { DataLength = 2048 }
            };
            columnOpts.Store.Remove(StandardColumn.Properties);
            columnOpts.Store.Add(StandardColumn.LogEvent);
            columnOpts.PrimaryKey = columnOpts.TimeStamp;
            columnOpts.TimeStamp.NonClusteredIndex = true;

            loggerConfiguration.WriteTo.Async(a => a.MSSqlServer(
                connectionString: loggingOptions.DatabaseConnectionString,
                sinkOptions: sinkOpts,
                columnOptions: columnOpts
            ));
        }

        // Create and configure the logger
        Log.Logger = loggerConfiguration.CreateLogger();

        // Register Serilog with DI
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(dispose: true);
        });

        // Register logging options for potential runtime configuration
        services.AddSingleton(loggingOptions);

        return services;
    }
}
