# Acontplus.Common.Logging

## Description
`Acontplus.Common.Logging` is a library that provides an advanced logging system for .NET applications. It allows storing logs in local files, Amazon S3, or a database, depending on the configuration defined in `appsettings.json`.

## Installation
To install the library, run the following command in the NuGet Package Manager Console:
```bash
Install-Package Acontplus.Common.Logging
```
```nuget 
dotnet add package Acontplus.Common.Logging
```

## Configuration
To enable and customize the logging system, edit the `appsettings.json` file and add the following section:

```json
"AdvancedLogging": {
    "EnableLocalFile": true,
    "Shared": false,
    "Buffered": true,
    "LocalFilePath": "logs/log-.log",
    "RollingInterval": "Day",
    "RetainedFileCountLimit": 7,
    "FileSizeLimitBytes": 10485760, // 10MB in bytes
    "EnableS3Logging": false,
    "S3BucketName": "my-application-logs",
    "S3AccessKey": "your-access-key",
    "S3SecretKey": "your-secret-key",
    "EnableDatabaseLogging": false,
    "DatabaseConnectionString": "Server=...",
    "TimeZoneId": "America/Guayaquil",
    "MinimumLogLevel": "Information"
}
```

### Configuration Options
- **EnableLocalFile** *(bool)*: Enables or disables storing logs in local files.
- **Shared** *(bool)*: Enables or disables shared log files.
- **Buffered** *(bool)*: Enables or disables buffered logging.
- **LocalFilePath** *(string)*: Path to the log file. It can include `{Date}` to generate a file per day.
- **RollingInterval** *(string)*: Interval to roll log files. Possible values: `Year`, `Month`, `Day`, `Hour`, `Minute`.
- **RetainedFileCountLimit** *(int)*: Number of log files to keep.
- **FileSizeLimitBytes** *(int)*: Maximum size of the log file in bytes.
- **EnableS3Logging** *(bool)*: Enables or disables storing logs in Amazon S3.
- **S3BucketName** *(string)*: Name of the S3 bucket where logs will be stored.
- **S3AccessKey** *(string)*: AWS access key for the S3 bucket.
- **S3SecretKey** *(string)*: AWS secret key for the S3 bucket.
- **EnableDatabaseLogging** *(bool)*: Enables or disables storing logs in a database.
- **DatabaseConnectionString** *(string)*: Connection string to the database where logs will be stored.
- **MinimumLogLevel** *(string)*: Minimum log level to be recorded. Possible values: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`.

## Usage

Once configured, the logging system will activate according to the settings defined in `appsettings.json`. It is recommended to check file access permissions, S3 configuration, and the database connection string to ensure proper functionality.

Inject in your services in program:
```csharp 
    builder.Services.AddAdvancedLogging(builder.Configuration);
```
## Requirements
- .NET 6 or higher
- Proper write permissions if `EnableLocalFile` is enabled.
- AWS account with S3 permissions if `EnableS3Logging` is enabled.
- Accessible database if `EnableDatabaseLogging` is enabled.

## Contributions
Contributions to improve this library are welcome. To report bugs or suggestions, open an issue in the official repository.

## License
This project is licensed under the MIT License - see the LICENSE file for details.

## Contact
If you have any questions or need support, please feel free to contact us.

- **Author:** Ivan Paz
- **Company:** Acontplus S.A.S.
- **Email:** ifer343@gmail.com

---
