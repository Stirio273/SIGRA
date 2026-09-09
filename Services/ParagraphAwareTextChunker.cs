using System.Text;

namespace SIGRA.Services;

public sealed class ParagraphAwareTextChunker : IDocumentChunker
{
    private readonly SimpleTextChunker _fallback = new();

    public IReadOnlyList<string> Chunk(string content, int maxTokensPerChunk = 300)
    {
        var maxWordsPerChunk = (int)(maxTokensPerChunk * 0.75);

        var paragraphs = content
            .Split(["\r\n\r\n", "\n\n"], StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .ToList();

        var chunks = new List<string>();
        var currentChunk = new StringBuilder();
        var currentWordCount = 0;

        foreach (var paragraph in paragraphs)
        {
            var paragraphWordCount = paragraph.Split(
                (char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;

            // A single paragraph longer than the limit still needs word-level splitting.
            if (paragraphWordCount > maxWordsPerChunk)
            {
                if (currentChunk.Length > 0)
                {
                    chunks.Add(currentChunk.ToString().Trim());
                    currentChunk.Clear();
                    currentWordCount = 0;
                }

                chunks.AddRange(_fallback.Chunk(paragraph, maxTokensPerChunk));
                continue;
            }

            if (currentWordCount + paragraphWordCount > maxWordsPerChunk)
            {
                chunks.Add(currentChunk.ToString().Trim());
                currentChunk.Clear();
                currentWordCount = 0;
            }

            currentChunk.AppendLine(paragraph);
            currentChunk.AppendLine();
            currentWordCount += paragraphWordCount;
        }

        if (currentChunk.Length > 0)
            chunks.Add(currentChunk.ToString().Trim());

        return chunks;
    }
}
