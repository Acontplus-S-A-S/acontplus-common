using Serilog.Core;
using Serilog.Events;

namespace Common.Logging;

public class LoggingOptions
{
    public bool EnableLocalFile { get; set; }
    public bool Buffered { get; set; }
    public bool Shared { get; set; } //If buffered is false, can set shared to true
    public string LocalFilePath { get; set; }
    public bool EnableS3Logging { get; set; }
    public string S3BucketName { get; set; }
    public string S3AccessKey { get; set; }
    public string S3SecretKey { get; set; }
    public bool EnableDatabaseLogging { get; set; }
    public string DatabaseConnectionString { get; set; }
    public LogEventLevel MinimumLogLevel { get; set; } = LogEventLevel.Information;
    public LoggingLevelSwitch LevelSwitch { get; set; } = new LoggingLevelSwitch(LogEventLevel.Information);

    public void UpdateLogLevel(LogEventLevel logEventLevel)
    {
        LevelSwitch.MinimumLevel = logEventLevel;
    }
}
