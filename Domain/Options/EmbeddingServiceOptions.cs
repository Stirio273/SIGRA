namespace SIGRA.Domain.Options;

public class EmbeddingServiceOptions
{
    public string BaseUrl { get; set; } = "http://localhost:8001";
    public int BatchSize { get; set; } = 32;
    public int TimeoutSeconds { get; set; } = 30;
}
