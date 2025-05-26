# Common.Core

A comprehensive .NET utility library providing common functionality for enterprise applications.

## Features

### Core Components

- **Base Entity System** - Common entity structures and interfaces
- **Error Logging** - Standardized error logging mechanisms
- **API Communication** - Request/Response models for API integration
- **AWS Integration** - AWS credentials management
- **S3 Operations** - S3 object handling and custom response processing
- **Email Handling** - Email processing utilities
- **File Management** - File handling and model processing

### Utility Components

#### File Operations

- **Compression Utils** - File compression and decompression
- **File Extensions** - File type handling and management
- **MIME Type Mapping** - File type detection and MIME type handling

#### Security

- **Encryption** - Data encryption/decryption utilities
- **External Validations** - Third-party validation integrations

#### Misc Utilities

- **Barcode Generation** - Barcode creation and processing
- **Custom Logging** - Enhanced logging capabilities
- **Enum Extensions** - Enhanced enum functionality
- **Picture Helper** - Image processing utilities
- **Text Handlers** - Text manipulation and processing

## Installation

```bash
dotnet add package Acontplus.Common.Core
```

## Usage Examples

### Base Entity Usage

```csharp
public class MyEntity : BaseEntity
{
    // Your entity properties
}
```

[//]: # (### Error Logging)

[//]: # (```csharp)

[//]: # (public void MyMethod&#40;&#41;)

[//]: # ({)

[//]: # (    try)

[//]: # (    {)

[//]: # (        // Your code)

[//]: # (    })

[//]: # (    catch &#40;Exception ex&#41;)

[//]: # (    {)

[//]: # (        ErrorLog.LogException&#40;ex&#41;;)

[//]: # (    })

[//]: # (})

[//]: # (```)

[//]: # ()

[//]: # (### AWS S3 Operations)

[//]: # (```csharp)

[//]: # (var credentials = new AwsCredentials)

[//]: # ({)

[//]: # (    AccessKey = "your-access-key",)

[//]: # (    SecretKey = "your-secret-key")

[//]: # (};)

[//]: # ()

[//]: # (// S3 operations)

[//]: # (```)

[//]: # ()

[//]: # (### Data Validation)

[//]: # (```csharp)

[//]: # (if &#40;DataValidation.IsValid&#40;myData&#41;&#41;)

[//]: # ({)

[//]: # (    // Process valid data)

[//]: # (})

[//]: # (```)

## Dependencies

- .NET Standard 2.0+
- AWS SDK for .NET (for AWS features)
- Additional dependencies can be found in the Dependencies folder

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

[MIT License]

## Support

For support, please [https://github.com/Acontplus-S-A-S/acontplus-common]