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
        // Read logging options from configuration
        var loggingOptions = new LoggingOptions();
        configuration.GetSection("AdvancedLogging").Bind(loggingOptions);

        // Create logger configuration
        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(loggingOptions.MinimumLogLevel)
            .WriteTo.Console()
            .Enrich.WithEnvironmentUserName()
            .Enrich.FromLogContext();

        // Add local file sink if enabled
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

        // Add S3 sink if enabled
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
                failureCallback: e => Console.WriteLine($"An error occured in my sink: {e.Message}")
            );
        }

        // Add SQL Server sink if enabled
        if (loggingOptions.EnableDatabaseLogging &&
            !string.IsNullOrEmpty(loggingOptions.DatabaseConnectionString))
        {
            //var logDB = @"Server=...";
            var sinkOpts = new MSSqlServerSinkOptions();
            sinkOpts.TableName = "Logs";
            sinkOpts.SchemaName = "Common";
            sinkOpts.AutoCreateSqlTable = true;
            sinkOpts.BatchPostingLimit = 1000;

            var columnOpts = new ColumnOptions();
            columnOpts.Id.DataType = SqlDbType.BigInt;
            columnOpts.Store.Remove(StandardColumn.Properties);
            columnOpts.Store.Add(StandardColumn.LogEvent);
            columnOpts.LogEvent.DataLength = 2048;
            columnOpts.PrimaryKey = columnOpts.TimeStamp;
            columnOpts.TimeStamp.NonClusteredIndex = true;

            loggerConfiguration.WriteTo.MSSqlServer(
                    connectionString: loggingOptions.DatabaseConnectionString,
                    sinkOptions: sinkOpts,
                    columnOptions: columnOpts
                );
        }

        // Create and configure the logger
        Log.Logger = loggerConfiguration.CreateLogger();
        //Log.Debug("This is a log for test db");

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

