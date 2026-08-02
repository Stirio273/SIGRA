using System.Text.RegularExpressions;
using MimeKit;

namespace SIGRA.Services.Helper;

public class EmailHelper
{
    public static string GetCleanBody(MimeMessage message)
    {
        var body = message.TextBody ?? message.HtmlBody ?? string.Empty;

        if (string.IsNullOrEmpty(body))
            return string.Empty;

        if (body.Contains('<') && body.Contains('>'))
        {
            body = Regex.Replace(body, "<.*?>", string.Empty);
        }

        var lines = body.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var result = new List<string>();

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith(">") ||
                IsQuoteHeaderLine(trimmed) ||
                trimmed.StartsWith("From:", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("Sent:", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("To:", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("Subject:", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }
            result.Add(line);
        }

        return string.Join(Environment.NewLine, result).Trim();
    }

    private static bool IsQuoteHeaderLine(string line)
    {
        var lower = line.ToLowerInvariant();
        return lower.StartsWith("on ") && lower.Contains("wrote:");
    }
}
