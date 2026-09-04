using System.Text;
using SIGRA.Data.Enums;
using SIGRA.Domain.AIsupport;

namespace SIGRA.Services;

public class TicketPromptBuilder : IPromptBuilder
{
    public string BuildSystemPrompt()
    {
        return """
            You are an internal support assistant helping an L2 technician
            investigate application incidents.

            Rules:
            - Use only the ticket information provided.
            - Do not invent facts, documents, or past tickets that were not given to you.
            - Clearly state when information is insufficient to determine a root cause.
            - Provide practical, step-by-step investigation guidance.
            - Do not claim certainty when the evidence is incomplete.
            - Respond in a neutral, professional tone.
            """;
    }

    public string BuildUserPrompt(
        TicketContext ticket,
        string technicianQuestion, IReadOnlyList<KnowledgeSearchResult> knowledgeResults)
    {
        var builder = new StringBuilder();

        builder.AppendLine("Ticket information:");
        builder.AppendLine($"ID: {ticket.IdTicket}");
        builder.AppendLine($"Title: {ticket.Title}");
        builder.AppendLine($"Application: {ticket.Application}");
        builder.AppendLine($"Category: {ticket.Category}");
        builder.AppendLine($"Status: {ticket.Status}");
        builder.AppendLine();
        builder.AppendLine("Description:");
        builder.AppendLine(ticket.Description);

        if (ticket.Comments.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Relevant comments (chronological):");

            foreach (var comment in ticket.Comments)
            {
                builder.AppendLine(
                    $"- [{"Unknown"}] {comment.Content}");
            }
        }

        builder.AppendLine();

        if (knowledgeResults.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Relevant internal knowledge:");

            foreach (var result in knowledgeResults)
            {
                var label = result.SourceType == KnowledgeSourceType.ResolvedTicket
                    ? $"Past resolved ticket ({ticket.Application})"
                    : $"Official {ticket.Application} documentation";

                var resolutionNote = result.ResolutionType switch
                {
                    ResolutionType.Workaround => " (WORKAROUND ONLY — root cause not fixed)",
                    ResolutionType.RootCauseFix => " (root cause fix)",
                    _ => ""
                };

                var recurrenceNote = result.RecurrenceCount is > 2
                    ? $" — this issue has recurred {result.RecurrenceCount} times."
                    : "";

                builder.AppendLine($"[{result.SourceId}] ({label}){resolutionNote}{recurrenceNote} {result.Title}");
                builder.AppendLine(result.Content);
                builder.AppendLine();
            }

            builder.AppendLine($"""
        Trust hierarchy for the sources above:
        1. Official {ticket.Application} documentation — authoritative for standard {ticket.Application} behavior.
        2. Past resolved tickets — reflect prior experience with {ticket.Application} only, 
           not guaranteed correctness.

        If a past resolved ticket is marked as a workaround and the issue has recurred 
        multiple times without a root-cause fix, you must state this pattern explicitly 
        to the technician, avoid presenting the workaround as a final solution, and set 
        "recommendedEscalation" to suggest root-cause investigation.

        Reference source IDs (e.g., [DOC-STOCK-001] or [INC-9931]) when used.
        """);
        }

        else
        {
            builder.AppendLine();
            builder.AppendLine("No relevant internal knowledge was found for this ticket.");
        }


        builder.AppendLine("Technician request:");
        builder.AppendLine(technicianQuestion);

        builder.AppendLine();
        builder.AppendLine("""
            Respond ONLY with a valid JSON object matching this schema:
            {
              "ticketUnderstanding": string,
              "suggestedSteps": string[],
              "possibleCauses": string[],
              "recommendedEscalation": string | null,
              "limitationOrUncertainty": string | null
            }
            """);

        return builder.ToString();
    }
}
