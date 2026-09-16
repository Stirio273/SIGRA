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

        var sanitized = SignatureAndQuoteCutoffPattern.Replace(content, string.Empty).TrimEnd();

        // Examples only: tune these patterns to your environment.
        sanitized = PasswordPattern().Replace(
            sanitized,
            "$1$2[REDACTED]");
        sanitized = BearerTokenPattern().Replace(
            sanitized,
            "Bearer [REDACTED]");
        sanitized = CredentialLabelPattern.Replace(sanitized, "[CREDENTIAL_REDACTED]");
        sanitized = ConnectionStringPattern.Replace(sanitized, "$1[REDACTED]");
        sanitized = EmailPattern.Replace(sanitized, "[EMAIL_REDACTED]");
        sanitized = PhonePattern.Replace(sanitized, "[PHONE_REDACTED]");
        sanitized = IpAddressPattern.Replace(sanitized, "[IP_ADDRESS_REDACTED]");

        return sanitized;
    }

    private static readonly Regex EmailPattern = new(
    @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}",
    RegexOptions.Compiled);

    private static readonly Regex PhonePattern = new(
    @"(\+?\d{1,3}[\s.-]?)?\(?\d{2,4}\)?[\s.-]?\d{3,4}[\s.-]?\d{3,4}",
    RegexOptions.Compiled);

    private static readonly Regex IpAddressPattern = new(
    @"\b(?:\d{1,3}\.){3}\d{1,3}\b",
    RegexOptions.Compiled);

    private static readonly Regex SignatureAndQuoteCutoffPattern = new(
    @"(?im)^\s*(merci|cordialement|bien à vous|bien cordialement|best regards|kind regards|" +
    @"warm regards|regards|sincerely|merci d'avance|-----original message-----|" +
    @"de\s*:|from\s*:|le\s.+\ba écrit\s*:).*",
    RegexOptions.Compiled | RegexOptions.Singleline);


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
    // [GeneratedRegex(
    //     @"(?i)(\b(password|pwd|passwd|api[_-]?key|secret|token|credential)\s*=\s*)([^;]+)",
    //     RegexOptions.Compiled)]
    // private static partial Regex ConnectionStringPasswordPattern();
    private static readonly Regex ConnectionStringPattern = new(
       @"(?i)(server|host|database|uid|user id|pwd|password)\s*=\s*[^;]+;?",
       RegexOptions.Compiled);

    private static readonly Regex CredentialLabelPattern = new(
        @"(?i)(password|pwd|passwd|api[_-]?key|secret|token|credential)s?\s*[:=]\s*\S+",
        RegexOptions.Compiled);
}
