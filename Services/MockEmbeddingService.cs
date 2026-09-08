using SIGRA.Services;

public sealed class MockEmbeddingService : IEmbeddingService
{
    public int Dimensions => 384;

    public Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        // Deterministic pseudo-embedding for testing pipeline wiring only —
        // not semantically meaningful.
        var rng = new Random(text.GetHashCode());
        var vector = new float[Dimensions];
        for (var i = 0; i < Dimensions; i++) vector[i] = (float)rng.NextDouble();
        return Task.FromResult(vector);
    }
}
