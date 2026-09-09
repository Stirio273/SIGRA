namespace SIGRA.Services;

public sealed class TextExtractionResult
{
    public bool Success { get; init; }
    public string? PlainText { get; init; }
    public string? FailureReason { get; init; }

    public static TextExtractionResult Ok(string text) =>
        new() { Success = true, PlainText = text };

    public static TextExtractionResult Fail(string reason) =>
        new() { Success = false, FailureReason = reason };
}

public interface IFileTextExtractor
{
    bool CanHandle(string fileName, string contentType);

    Task<TextExtractionResult> ExtractAsync(Stream fileStream, CancellationToken cancellationToken = default);
}
