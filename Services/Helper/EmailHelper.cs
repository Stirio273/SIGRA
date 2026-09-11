using System.Text.RegularExpressions;
using MimeKit;

namespace SIGRA.Services.Helper;

public class EmailHelper
{
    public static string GetCleanBody(MimeMessage message)
    {
        try
        {
            var bodyType = message.Body?.ContentType;
            var body = message.TextBody ?? message.HtmlBody ?? string.Empty;

            if (string.IsNullOrEmpty(body))
                return string.Empty;


            if (bodyType != null && bodyType.IsMimeType("text", "html"))
            {
                // Step 1: Remove HTML quote containers
                var strippedHtml = HtmlEmailCleanerHelper.CleanHtmlBody(body);

                // Step 2: Convert to plain text for final cleanup
                var plainText = HtmlEmailCleanerHelper.ExtractPlainText(strippedHtml);

                // Step 3: Apply plain text patterns as fallback
                body = PlainTextEmailCleanerHelper.CleanPlainTextBody(plainText);
            }
            else
            {
                // Direct plain text cleaning
                body = PlainTextEmailCleanerHelper.CleanPlainTextBody(body);
            }

            return body;
        }
        catch (System.Exception)
        {
            throw;
        }

    }

    private static bool IsQuoteHeaderLine(string line)
    {
        var lower = line.ToLowerInvariant();
        return lower.StartsWith("on ") && lower.Contains("wrote:");
    }

    public static string GetExtensionFromMimeType(string mimeType)
    {
        // Association basique pour les cas courants si le nom est null
        return mimeType.ToLower() switch
        {
            "application/pdf" => ".pdf",
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/png" => ".png",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => ".xlsx",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
            "text/plain" => ".txt",
            "text/csv" => ".csv",
            _ => ".dat" // Extension générique par défaut
        };
    }
}
