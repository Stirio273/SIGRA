namespace SIGRA.Services;

public sealed class TextExtractionService
{
    private readonly IReadOnlyList<IFileTextExtractor> _extractors;

    public TextExtractionService(IEnumerable<IFileTextExtractor> extractors)
    {
        _extractors = extractors.ToList();
    }

    public async Task<TextExtractionResult> ExtractAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        var extractor = _extractors.FirstOrDefault(e => e.CanHandle(file.FileName, file.ContentType));

        if (extractor is null)
        {
            return TextExtractionResult.Fail(
                $"Unsupported file type: {Path.GetExtension(file.FileName)}");
        }

        await using var stream = file.OpenReadStream();

        try
        {
            return await extractor.ExtractAsync(stream, cancellationToken);
        }
        catch (Exception ex)
        {
            return TextExtractionResult.Fail($"Extraction failed: {ex.Message}");
        }
    }
}
