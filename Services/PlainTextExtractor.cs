namespace SIGRA.Services;

public sealed class PlainTextExtractor : IFileTextExtractor
{
    public bool CanHandle(string fileName, string contentType) =>
        fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ||
        fileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase);

    public async Task<TextExtractionResult> ExtractAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(fileStream);
        var text = await reader.ReadToEndAsync(cancellationToken);
        return TextExtractionResult.Ok(text);
    }
}