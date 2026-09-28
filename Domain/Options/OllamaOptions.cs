namespace SIGRA.Domain.Options;

public sealed class OllamaOptions
{
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string GenerationModel { get; set; } = "mistral";
}
