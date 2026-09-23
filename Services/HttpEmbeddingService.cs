using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SIGRA.Domain.Options;

namespace SIGRA.Services;

public sealed class HttpEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly EmbeddingServiceOptions _options;
    private readonly ILogger<HttpEmbeddingService> _logger;

    public int Dimensions { get; }

    public HttpEmbeddingService(
        HttpClient httpClient,
        IOptions<EmbeddingServiceOptions> options,
        ILogger<HttpEmbeddingService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
        }

        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

        Dimensions = 1024;
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
    {
        using var response = await SendWithRetryAsync(
            () => _httpClient.PostAsJsonAsync("embed", new { text = text }, cancellationToken),
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload = await DeserializeAsync<EmbedSingleResponse>(response.Content, cancellationToken);
        return payload?.Embedding ?? throw new InvalidOperationException("Empty embedding response from embedding service.");
    }

    public async Task<float[][]> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
    {
        var textList = texts.ToList();
        if (textList.Count == 0)
            return Array.Empty<float[]>();

        using var response = await SendWithRetryAsync(
            () => _httpClient.PostAsJsonAsync("embed/batch", new { texts = textList }, cancellationToken),
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload = await DeserializeAsync<EmbedBatchResponse>(response.Content, cancellationToken);
        return payload?.Embeddings ?? throw new InvalidOperationException("Empty batch embedding response from embedding service.");
    }

    private async Task<HttpResponseMessage> SendWithRetryAsync(
        Func<Task<HttpResponseMessage>> send,
        CancellationToken cancellationToken)
    {
        const int maxRetries = 2;
        for (var attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await send();
            }
            catch (HttpRequestException ex) when (attempt < maxRetries)
            {
                _logger.LogWarning(ex, "Embedding service request failed (attempt {Attempt}/{Max}).", attempt + 1, maxRetries);
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        return await send();
    }

    private static async Task<T?> DeserializeAsync<T>(HttpContent content, CancellationToken cancellationToken)
    {
        await using var stream = await content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken);
    }
}

internal record EmbedSingleResponse(float[] Embedding);
internal record EmbedBatchResponse(float[][] Embeddings);
