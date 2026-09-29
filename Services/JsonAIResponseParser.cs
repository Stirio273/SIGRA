using System.Text.Json;
using SIGRA.Domain.AIsupport;

namespace SIGRA.Services;

public sealed class JsonAiResponseParser : IAIResponseParser
{
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

    public AISupportResponse Parse(string rawLlmResponse)
    {
        if (string.IsNullOrWhiteSpace(rawLlmResponse))
        {
            return Fallback("Empty model response.");
        }

        var json = ExtractJson(rawLlmResponse);

        if (json is null)
        {
            return Fallback("No JSON object found in model output.");
        }

        try
        {
            var dto = JsonSerializer.Deserialize<AiSupportResponseDto>(json, JsonOptions);

            if (dto is null)
            {
                return Fallback("Deserialized response was null.");
            }

            return new AISupportResponse
            {
                TicketUnderstanding = dto.TicketUnderstanding ?? string.Empty,
                SuggestedSteps = dto.SuggestedSteps ?? Array.Empty<string>(),
                PossibleCauses = dto.PossibleCauses ?? Array.Empty<string>(),
                RecommendedEscalation = dto.RecommendedEscalation,
                LimitationOrUncertainty = dto.LimitationOrUncertainty,
                Sources = Array.Empty<AISourceReference>()
            };
        }
        catch (JsonException ex)
        {
            return Fallback($"JSON parsing failed: {ex.Message}");
        }
    }

    private static string? ExtractJson(string raw)
    {
        var trimmed = raw.Trim();

        if (trimmed.StartsWith("```"))
        {
            var firstNewline = trimmed.IndexOf('\n');
            if (firstNewline > 0)
            {
                trimmed = trimmed.Substring(firstNewline + 1);
            }

            if (trimmed.EndsWith("```"))
            {
                trimmed = trimmed[..^3];
            }

            trimmed = trimmed.Trim();
        }

        if (!trimmed.StartsWith("{"))
        {
            return null;
        }

        return trimmed;
    }

    private static AISupportResponse Fallback(string reason)
    {
        return new AISupportResponse
        {
            TicketUnderstanding = "The assistant response could not be parsed as structured data.",
            SuggestedSteps = Array.Empty<string>(),
            PossibleCauses = Array.Empty<string>(),
            LimitationOrUncertainty =
                $"The raw model output was not in the expected format. {reason} Manual review of the ticket is recommended.",
            Sources = Array.Empty<AISourceReference>()
        };
    }

    private sealed class AiSupportResponseDto
    {
        public string? TicketUnderstanding { get; set; }
        public string[]? SuggestedSteps { get; set; }
        public string[]? PossibleCauses { get; set; }
        public string? RecommendedEscalation { get; set; }
        public string? LimitationOrUncertainty { get; set; }
    }
}
