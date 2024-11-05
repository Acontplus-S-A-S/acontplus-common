# Acontplus.Common.Infrastructure

A .NET library providing common infrastructure components for database access and data operations.

## Overview

Acontplus.Common.Infrastructure is a utility library that provides common infrastructure components and database access functionality for .NET applications. It's built on .NET 8.0 and integrates with Entity Framework Core.

## Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package Acontplus.Common.Infrastructure
```

Or via the NuGet Package Manager Console:

```powershell
Install-Package Acontplus.Common.Infrastructure
```

## Dependencies

- FastMember (1.5.0)
- Microsoft.Data.SqlClient (5.2.2)
- Microsoft.EntityFrameworkCore (8.0.10)
- Microsoft.EntityFrameworkCore.SqlServer (8.0.10)

## Features

- Database context management
- Repository pattern implementation
- SQL Server integration
- Data reader mapping utilities
- Parameter handling helpers

## Project Structure

- BaseContext.cs - Base database context implementation
- DbContextFactory.cs - Factory for creating database contexts
- Repository/ - Repository pattern implementations
  - AdoRepository.cs - ADO.NET based repository
  - AdoSqlServer.cs - SQL Server specific implementations
  - IAdoRepository.cs - Repository interfaces
- Utils/ - Utility classes for data operations

## License

This project is licensed under the MIT License.

## Author

Ivan Paz

## Company

Acontplus S.A.S.

## Repository

[GitHub Repository](https://github.com/Acontplus-S-A-S/acontplus-common)

## Tags

database;ado-net;data-access;sql;orm;micro-orm;query;crud

## Contributing
We welcome contributions! Please submit any issues or feature requests via our GitHub repository, or feel free to fork the project and submit pull requests.

## License
This project is licensed under the MIT License - see the LICENSE file for details.

## Contact
If you have any questions or need support, please feel free to contact us.

- **Author:** Ivan Paz
- **Company:** Acontplus S.A.S.
- **Email:** ifer343@gmail.com