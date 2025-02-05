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
        // Enable Serilog self-logging for debugging
        Serilog.Debugging.SelfLog.Enable(Console.WriteLine);

        // Read logging options from configuration
        var loggingOptions = new LoggingOptions();
        configuration.GetSection("AdvancedLogging").Bind(loggingOptions);

        // Create logger configuration
        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(loggingOptions.MinimumLogLevel)
            .WriteTo.Console(new CompactJsonFormatter()) // Use structured logging for better performance
            .Enrich.WithEnvironmentUserName()
            .Enrich.FromLogContext();

        // Add asynchronous file sink if enabled
        if (loggingOptions.EnableLocalFile && !string.IsNullOrEmpty(loggingOptions.LocalFilePath))
        {
            loggerConfiguration.WriteTo.Async(a => a.File(
                path: loggingOptions.LocalFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
                buffered: true, // Enable buffering for better performance
                shared: false, // Allow multiple processes to write to the same log file
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            ));
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
