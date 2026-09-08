namespace SIGRA.Services;

public interface IEmbeddingService
{
    Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default);

    int Dimensions { get; }
}
