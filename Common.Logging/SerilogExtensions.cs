namespace Common.Logging;

public static class SerilogExtensions
{
    public static IServiceCollection AddAdvancedLogging(this IServiceCollection services, IConfiguration configuration, Action<LoggerConfiguration> configureLogger = null)
    {
        var environment = GetEnvironmentName(configuration);

        // Enable Serilog self-logging for debugging
        Serilog.Debugging.SelfLog.Enable(Console.WriteLine);

        // Read logging options from configuration
        var loggingOptions = new LoggingOptions();
        configuration.GetSection("AdvancedLogging").Bind(loggingOptions);

        // Configure Serilog
        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(loggingOptions.MinimumLogLevel)
            .Enrich.WithEnvironmentUserName()
            .Enrich.FromLogContext()
            .Enrich.With(new CustomTimeZoneEnricher(loggingOptions.TimeZoneId));

        // Add console logging for development
        if (environment == Environments.Development)
        {
            loggerConfiguration.WriteTo.Console(
                outputTemplate: "{CustomTimestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            );
        }

        // Configure local file logging
        if (loggingOptions.EnableLocalFile && !string.IsNullOrEmpty(loggingOptions.LocalFilePath))
        {
            ConfigureLocalLogging(loggerConfiguration, loggingOptions, environment);
        }

        // Configure S3 logging
        if (loggingOptions.EnableS3Logging)
        {
            ConfigureS3Logging(loggerConfiguration, loggingOptions);
        }

        // Configure SQL Server logging
        if (loggingOptions.EnableDatabaseLogging)
        {
            ConfigureDatabaseLogging(loggerConfiguration, loggingOptions);
        }

        // Allow custom configuration
        configureLogger?.Invoke(loggerConfiguration);

        // Create and configure the logger
        Log.Logger = loggerConfiguration.CreateLogger();

        // Register Serilog in DI
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(dispose: true);
        });

        services.AddSingleton(loggingOptions);

        return services;
    }

    private static string GetEnvironmentName(IConfiguration configuration)
    {
        return Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
               Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
               Environments.Production;
    }

    private static void ConfigureLocalLogging(LoggerConfiguration loggerConfiguration, LoggingOptions options, string environment)
    {
        var rollingInterval = (RollingInterval)Enum.Parse(typeof(RollingInterval), options.RollingInterval ?? "Day", true);
        var retainedFileCountLimit = options.RetainedFileCountLimit ?? 7;
        var fileSizeLimitBytes = options.FileSizeLimitBytes ?? 10 * 1024 * 1024;

        if (environment == Environments.Development)
        {
            loggerConfiguration.WriteTo.Async(a => a.File(
                path: options.LocalFilePath,
                rollingInterval: rollingInterval,
                retainedFileCountLimit: retainedFileCountLimit,
                fileSizeLimitBytes: fileSizeLimitBytes,
                encoding: System.Text.Encoding.UTF8,
                buffered: true,
                shared: false,
                outputTemplate: "{CustomTimestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            ));
        }
        else
        {
            loggerConfiguration.WriteTo.Async(a => a.File(
                formatter: new CompactJsonFormatter(),
                path: options.LocalFilePath,
                rollingInterval: rollingInterval,
                retainedFileCountLimit: retainedFileCountLimit,
                fileSizeLimitBytes: fileSizeLimitBytes,
                encoding: System.Text.Encoding.UTF8,
                buffered: true,
                shared: false
            ));
        }
    }

    private static void ConfigureS3Logging(LoggerConfiguration loggerConfiguration, LoggingOptions options)
    {
        if (string.IsNullOrEmpty(options.S3BucketName) || string.IsNullOrEmpty(options.S3AccessKey) || string.IsNullOrEmpty(options.S3SecretKey))
        {
            Log.Warning("S3 logging is enabled but required settings are missing. Disabling S3 logging.");
            return;
        }

        loggerConfiguration.WriteTo.Async(a => a.AmazonS3(
            path: options.LocalFilePath,
            bucketName: options.S3BucketName,
            Amazon.RegionEndpoint.USEast1,
            awsAccessKeyId: options.S3AccessKey,
            awsSecretAccessKey: options.S3SecretKey,
            encoding: System.Text.Encoding.UTF8,
            outputTemplate: "{CustomTimestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
            rollingInterval: Serilog.Sinks.AmazonS3.RollingInterval.Minute,
            failureCallback: e => Console.WriteLine($"An error occurred in the S3 sink: {e.Message}")
        ));
    }

    private static void ConfigureDatabaseLogging(LoggerConfiguration loggerConfiguration, LoggingOptions options)
    {
        if (string.IsNullOrEmpty(options.DatabaseConnectionString))
        {
            Log.Warning("Database logging is enabled but the connection string is missing. Disabling database logging.");
            return;
        }

        var sinkOpts = new MSSqlServerSinkOptions
        {
            TableName = "Logs",
            SchemaName = "Common",
            AutoCreateSqlTable = true,
            BatchPostingLimit = 1000,
            BatchPeriod = TimeSpan.FromSeconds(5),
            EagerlyEmitFirstEvent = false  // Add this line to prevent issues with initial batching
        };

        var columnOpts = new ColumnOptions
        {
            Id = { DataType = SqlDbType.BigInt },
            LogEvent = { DataLength = 2048 }
        };

        // Make sure Id is auto-incremented
        columnOpts.Id.NonClusteredIndex = false;
        columnOpts.Id.AllowNull = false;

        // This is critical - use SQL Server's identity column for the primary key
        columnOpts.AdditionalColumns = new List<SqlColumn>
            {
                new SqlColumn
                {
                    ColumnName = "Id",
                    DataType = SqlDbType.BigInt,
                    AllowNull = false,
                    DataLength = -1,
                    NonClusteredIndex = false
                }
            };

        columnOpts.Store.Remove(StandardColumn.Properties);
        columnOpts.Store.Add(StandardColumn.LogEvent);
        columnOpts.PrimaryKey = columnOpts.Id;
        columnOpts.TimeStamp.NonClusteredIndex = true;

        loggerConfiguration.WriteTo.Async(a => a.MSSqlServer(
            connectionString: options.DatabaseConnectionString,
            sinkOptions: sinkOpts,
            columnOptions: columnOpts
        ));
    }
}
