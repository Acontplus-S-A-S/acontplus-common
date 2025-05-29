using System.Diagnostics;

namespace Common.Infrastructure.Helpers;

public static class DiagnosticConfig
{
    public static readonly ActivitySource ActivitySource = new("Repository");
}
