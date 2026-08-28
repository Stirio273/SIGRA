using SIGRA.Domain.AIsupport;

namespace SIGRA.Services;


public sealed class PlaceholderAISupportAssistant : IAISupportAssistant
{
    public Task<AISupportResponse> GetAssistanceAsync(
        TicketContext ticket,
        AISupportRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = new AISupportResponse
        {
            TicketUnderstanding =
                $"The ticket appears to concern: {ticket.Title}. " +
                "AI knowledge retrieval has not yet been connected.",

            SuggestedSteps = new[]
            {
                "Review the ticket description and attached error details.",
                "Confirm the affected ERPNext module, document type, item, warehouse, and transaction date.",
                "Check whether a similar resolved ticket exists."
            },

            PossibleCauses = Array.Empty<string>(),

            LimitationOrUncertainty =
                "This is a temporary placeholder response. " +
                "It does not yet search internal documents or historical tickets.",

            Sources = Array.Empty<AISourceReference>()
        };

        return Task.FromResult(response);
    }
}
