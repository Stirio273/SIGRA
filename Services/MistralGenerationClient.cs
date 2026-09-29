using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SIGRA.Domain.Options;

namespace SIGRA.Services;

public sealed class MistralGenerationClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly ILogger<MistralGenerationClient> _logger;
    private readonly MistralOptions _options;

    public MistralGenerationClient(
        HttpClient httpClient,
        IOptions<MistralOptions> options,
        ILogger<MistralGenerationClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _model = _options.Model;
        _logger = logger;

        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
        }

        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.MISTRAL_API_KEY);
    }

    public async Task<string> GetCompletionAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = 0.1,
            stream = false,
            response_format = new { type = "json_object" }
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "chat/completions",
            requestBody,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content
            .ReadFromJsonAsync<MistralChatResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Empty response from Mistral.");

        return payload.Choices[0].Message.Content;
    }

    private sealed class MistralChatResponse
    {
        public List<MistralChoice> Choices { get; set; } = new();
    }

    private sealed class MistralChoice
    {
        public MistralMessage Message { get; set; } = new();
    }

    private sealed class MistralMessage
    {
        public string Content { get; set; } = string.Empty;
    }
}
