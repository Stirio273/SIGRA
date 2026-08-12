using HtmlAgilityPack;

namespace SIGRA.Services.Helper;

public class HtmlEmailCleanerHelper
{
    // CSS selectors / patterns for known quote containers
    private static readonly string[] QuoteSelectors = new[]
    {
        "//div[@class='gmail_quote']",
        "//div[@id='divRplyFwdMsg']",
        "//div[contains(@class, 'OutlookMessageHeader')]",
        "//blockquote[@type='cite']",
        "//blockquote[contains(@class, 'gmail_quote')]",
        "//hr[@id='appendonsend']",
        "//div[@id='mail-editor-reference-message-container']" // Outlook new
    };

    public static string CleanHtmlBody(string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(htmlBody))
            return string.Empty;

        var doc = new HtmlDocument();
        doc.LoadHtml(htmlBody);

        // Remove all known quote containers
        foreach (var selector in QuoteSelectors)
        {
            var nodes = doc.DocumentNode.SelectNodes(selector);
            if (nodes == null) continue;

            foreach (var node in nodes)
            {
                // For <hr id="appendonsend">, remove everything AFTER it too
                if (node.Name == "hr")
                {
                    RemoveNodeAndFollowingSiblings(node);
                }
                else
                {
                    node.Remove();
                }
            }
        }

        // Extract clean text
        var cleanHtml = doc.DocumentNode.OuterHtml.Trim();
        return cleanHtml;
    }

    private static void RemoveNodeAndFollowingSiblings(HtmlNode node)
    {
        var parent = node.ParentNode;
        var siblingsToRemove = new List<HtmlNode>();

        var current = node;
        while (current != null)
        {
            siblingsToRemove.Add(current);
            current = current.NextSibling;
        }

        foreach (var sibling in siblingsToRemove)
            parent?.RemoveChild(sibling);
    }

    // Optional: Convert cleaned HTML to plain text
    public static string ExtractPlainText(string htmlBody)
    {
        var cleanedHtml = CleanHtmlBody(htmlBody);
        var doc = new HtmlDocument();
        doc.LoadHtml(cleanedHtml);
        return doc.DocumentNode.InnerText.Trim();
    }
}
