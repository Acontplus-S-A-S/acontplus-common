using Serilog.Events;

namespace Common.Logging;

public class LoggingOptions
{
    public bool EnableLocalFile { get; set; }
    public string LocalFilePath { get; set; }
    public bool EnableS3Logging { get; set; }
    public string S3BucketName { get; set; }
    public string S3AccessKey { get; set; }
    public string S3SecretKey { get; set; }
    public bool EnableDatabaseLogging { get; set; }
    public string DatabaseConnectionString { get; set; }
    public LogEventLevel MinimumLogLevel { get; set; } = LogEventLevel.Information;
}
