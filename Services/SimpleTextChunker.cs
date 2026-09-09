namespace SIGRA.Services;

public sealed class SimpleTextChunker : IDocumentChunker
{
    private const int DefaultOverlapWords = 40;

    public IReadOnlyList<string> Chunk(string content, int maxTokensPerChunk = 300)
    {
        if (string.IsNullOrWhiteSpace(content))
            return [];

        // Approximation: 1 token ≈ 0.75 words for English text.
        var maxWordsPerChunk = (int)(maxTokensPerChunk * 0.75);

        var words = content
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        if (words.Count <= maxWordsPerChunk)
            return [content.Trim()];

        var chunks = new List<string>();
        var start = 0;

        while (start < words.Count)
        {
            var length = Math.Min(maxWordsPerChunk, words.Count - start);
            var chunkWords = words.GetRange(start, length);
            chunks.Add(string.Join(' ', chunkWords));

            if (start + length >= words.Count)
                break;

            // Move forward, but re-include the last DefaultOverlapWords 
            // words so context isn't abruptly cut at chunk boundaries.
            start += maxWordsPerChunk - DefaultOverlapWords;
        }

        return chunks;
    }
}
