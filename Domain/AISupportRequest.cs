namespace SIGRA.Domain;

public sealed class AISupportRequest
{
    public required string TechnicianQuestion { get; init; }

    public bool IncludeTicketComments { get; init; } = true;

    // Examples: "ERPNext", "Stock", "Buying"
    public IReadOnlyList<string> PreferredKnowledgeDomains { get; init; }
        = Array.Empty<string>();
}
