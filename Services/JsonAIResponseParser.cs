using SIGRA.Domain.AIsupport;

namespace SIGRA.Services;

public sealed class JsonAiResponseParser : IAIResponseParser
{
    public AISupportResponse Parse(string rawLlmResponse)
    {
        return new AISupportResponse
        {
            TicketUnderstanding = "The assistant response could not be parsed as structured data.",
            SuggestedSteps = [],
            PossibleCauses = [],
            LimitationOrUncertainty =
                "The raw model output was not in the expected format. " +
                "Manual review of the ticket is recommended.",
            Sources = []
        };
    }
}
