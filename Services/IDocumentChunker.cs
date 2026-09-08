namespace SIGRA.Services;

public interface IDocumentChunker
{
    IReadOnlyList<string> Chunk(string content, int maxTokensPerChunk = 300);
}
