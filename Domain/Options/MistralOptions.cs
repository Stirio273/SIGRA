namespace SIGRA.Domain.Options;

public sealed class MistralOptions
{
    public string MISTRAL_API_KEY { get; set; } = string.Empty;
    public string Model { get; set; } = "mistral-small-latest";
    public string BaseUrl { get; set; } = "https://api.mistral.ai/v1";
    public int TimeoutSeconds { get; set; } = 60;
}
