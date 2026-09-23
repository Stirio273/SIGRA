using SIGRA.Services;

public sealed class MockEmbeddingService : IEmbeddingService
{
    public int Dimensions => 384;

    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        var rng = new Random(text.GetHashCode());
        var vector = new float[Dimensions];
        for (var i = 0; i < Dimensions; i++) vector[i] = (float)rng.NextDouble();
        return Task.FromResult(vector);
    }

    public async Task<float[][]> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
    {
        var tasks = texts.Select(t => EmbedAsync(t, cancellationToken)).ToArray();
        var embeddings = await Task.WhenAll(tasks);
        return embeddings;
    }
}
