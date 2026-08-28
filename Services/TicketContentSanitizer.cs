using System.Text.RegularExpressions;

namespace SIGRA.Services;

public sealed partial class TicketContentSanitizer : ITicketContentSanitizer
{
    public string Sanitize(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        var sanitized = content;

        // Examples only: tune these patterns to your environment.
        sanitized = PasswordPattern().Replace(
            sanitized,
            "$1$2[REDACTED]");

        sanitized = BearerTokenPattern().Replace(
            sanitized,
            "Bearer [REDACTED]");

        sanitized = ConnectionStringPasswordPattern().Replace(
            sanitized,
            "$1[REDACTED]");

        return sanitized;
    }

    // Example: Password=secret or password: secret
    [GeneratedRegex(
        @"(?i)\b(password|pwd)(\s*[:=]\s*)([^\s;,\r\n]+)",
        RegexOptions.Compiled)]
    private static partial Regex PasswordPattern();

    // Example: Authorization: Bearer eyJ...
    [GeneratedRegex(
        @"(?i)\bBearer\s+[A-Za-z0-9\-_\.=]+",
        RegexOptions.Compiled)]
    private static partial Regex BearerTokenPattern();

    // Example: Password=my-secret inside a connection string
    [GeneratedRegex(
        @"(?i)(\b(password|pwd)\s*=\s*)([^;]+)",
        RegexOptions.Compiled)]
    private static partial Regex ConnectionStringPasswordPattern();
}
