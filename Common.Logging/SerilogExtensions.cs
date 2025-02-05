using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.MSSqlServer;

namespace Common.Logging;

public static class SerilogExtensions
{
    public static IServiceCollection AddAdvancedLogging(this IServiceCollection services, IConfiguration configuration)
    {
        var loggingOptions = new LoggingOptions();
        configuration.GetSection("AdvancedLogging").Bind(loggingOptions);

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(loggingOptions.MinimumLogLevel)
            .WriteTo.Console()
            .Enrich.WithEnvironmentUserName()
            .Enrich.FromLogContext();

        if (loggingOptions.EnableLocalFile && !string.IsNullOrEmpty(loggingOptions.LocalFilePath))
        {
            loggerConfiguration.WriteTo.File(
                path: loggingOptions.LocalFilePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            );
        }

        if (loggingOptions.EnableS3Logging &&
            !string.IsNullOrEmpty(loggingOptions.S3BucketName) &&
            !string.IsNullOrEmpty(loggingOptions.S3AccessKey) &&
            !string.IsNullOrEmpty(loggingOptions.S3SecretKey))
        {
            var levelSwitch = new LoggingLevelSwitch();

            loggerConfiguration.WriteTo.AmazonS3(
                path: "log.text",
                bucketName: loggingOptions.S3BucketName,
                Amazon.RegionEndpoint.USEast1,
                awsAccessKeyId: loggingOptions.S3AccessKey,
                awsSecretAccessKey: loggingOptions.S3SecretKey,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                levelSwitch: levelSwitch,
                rollingInterval: Serilog.Sinks.AmazonS3.RollingInterval.Minute,
                failureCallback: e => Console.WriteLine($"An error occurred in my sink: {e.Message}")
            );
        }

        if (loggingOptions.EnableDatabaseLogging &&
            !string.IsNullOrEmpty(loggingOptions.DatabaseConnectionString))
        {
            var sinkOpts = new MSSqlServerSinkOptions
            {
                TableName = "Logs",
                SchemaName = "Common",
                AutoCreateSqlTable = true,
                BatchPostingLimit = 1000
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

            loggerConfiguration.WriteTo.MSSqlServer(
                connectionString: loggingOptions.DatabaseConnectionString,
                sinkOptions: sinkOpts,
                columnOptions: columnOpts
            );
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(dispose: true);
        });

        services.AddSingleton(loggingOptions);

        return services;
    }
}

