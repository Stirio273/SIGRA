using System.Text.RegularExpressions;

namespace SIGRA.Services.Helper;

public class PlainTextEmailCleanerHelper
{
    // Common patterns indicating start of quoted content
    private static readonly Regex[] QuotePatterns = new[]
    {
        // "On Mon, Jan 15, 2024 at 3:00 PM John Doe <john@example.com> wrote:"
        new Regex(@"^On\s.+wrote:\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase),

        // "-----Original Message-----"
        new Regex(@"^-{3,}\s*Original Message\s*-{3,}", RegexOptions.Multiline | RegexOptions.IgnoreCase),

        // "________________________________" (Outlook separator)
        new Regex(@"^_{10,}\s*$", RegexOptions.Multiline),

        // "---------- Forwarded message ---------"
        new Regex(@"^-{3,}\s*Forwarded message\s*-{3,}", RegexOptions.Multiline | RegexOptions.IgnoreCase),

        // French Outlook: "De : ... Envoyé : ... À : ..."
        new Regex(@"^De\s?:\s.+$", RegexOptions.Multiline | RegexOptions.IgnoreCase),

        // "From: ... Sent: ... To: ..."
        new Regex(@"^From:\s.+$", RegexOptions.Multiline | RegexOptions.IgnoreCase),

        // Lines starting with ">" (quoted reply)
        new Regex(@"^>.*$", RegexOptions.Multiline)
    };

    public static string CleanPlainTextBody(string textBody)
    {
        if (string.IsNullOrWhiteSpace(textBody))
            return string.Empty;

        var lines = textBody.Split('\n');
        var cleanLines = new List<string>();

        foreach (var line in lines)
        {
            // Stop processing once we hit a quote marker
            if (IsQuoteMarkerLine(line))
                break;

            cleanLines.Add(line);
        }

        return string.Join("\n", cleanLines).Trim();
    }

    private static bool IsQuoteMarkerLine(string line)
    {
        foreach (var pattern in QuotePatterns)
        {
            if (pattern.IsMatch(line))
                return true;
        }
        return false;
    }
}
