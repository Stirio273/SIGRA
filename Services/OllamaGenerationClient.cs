using System.Text.Json;
using Microsoft.Extensions.Options;
using SIGRA.Domain.Options;

namespace SIGRA.Services;

public sealed class OllamaGenerationClient : ILlmClient
{
    private readonly HttpClient _httpClient;
    private readonly string _model;

    public OllamaGenerationClient(HttpClient httpClient, IOptions<OllamaOptions> options)
    {
        _httpClient = httpClient;
        _model = options.Value.GenerationModel;
    }

    public async Task<string> GetCompletionAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            format = "json",
            stream = false
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "/api/chat", requestBody, cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content
            .ReadFromJsonAsync<OllamaChatResponse>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Empty response from Ollama.");

        var rawContent = payload.Message.Content;

        try
        {
            // return JsonSerializer.Deserialize<TicketAnalysisResult>(
            //     rawContent,
            //     new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            //     ?? throw new InvalidOperationException("Model returned null result.");
            return rawContent;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Model output was not valid JSON matching the expected schema: {rawContent}", ex);
        }
    }

    private sealed record OllamaChatResponse(OllamaMessage Message);
    private sealed record OllamaMessage(string Role, string Content);

    public sealed record TicketAnalysisResult(
    string TicketUnderstanding,
    IReadOnlyList<string> SuggestedSteps,
    IReadOnlyList<string> PossibleCauses,
    string? RecommendedEscalation,
    string? LimitationOrUncertainty);
}