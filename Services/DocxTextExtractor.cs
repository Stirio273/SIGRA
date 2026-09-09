namespace SIGRA.Services;

public sealed class DocxTextExtractor : IFileTextExtractor
{
    public bool CanHandle(string fileName, string contentType) =>
        fileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase);

    public Task<TextExtractionResult> ExtractAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        using var wordDoc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(fileStream, false);

        var body = wordDoc.MainDocumentPart?.Document.Body;
        var text = body?.InnerText ?? string.Empty;

        return Task.FromResult(string.IsNullOrWhiteSpace(text)
            ? TextExtractionResult.Fail("No text content found in document.")
            : TextExtractionResult.Ok(text));
    }
}
