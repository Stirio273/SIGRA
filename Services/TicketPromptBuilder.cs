using System.Text;
using SIGRA.Data.Enums;
using SIGRA.Date.Enums;
using SIGRA.Domain.AIsupport;

namespace SIGRA.Services;

public class TicketPromptBuilder : IPromptBuilder
{
    public string BuildSystemPrompt()
    {
        return """
             Tu es un assistant de support interne qui aide un technicien de niveau 2
            à diagnostiquer des incidents applicatifs.

            Règles :
            - Réponds uniquement en français, quelle que soit la langue du ticket, 
            des commentaires ou des documents fournis.
            - Utilise uniquement les informations fournies dans le ticket et le contexte associé.
            - N'invente jamais de faits, de documents ou de tickets antérieurs qui ne t'ont pas été fournis.
            - Indique clairement lorsque les informations sont insuffisantes pour déterminer une cause racine.
            - Donne des conseils d'investigation pratiques, étape par étape.
            - N'affirme jamais une certitude lorsque les preuves sont incomplètes.
            - Réponds sur un ton neutre et professionnel.
            - Lorsque plusieurs sources sont fournies, privilégie la documentation officielle 
            par rapport aux tickets résolus antérieurs, qui ne garantissent pas une solution correcte.
            - Cite les identifiants de source (ex. [DOC-STOCK-001] ou [INC-9931]) lorsque tu utilises une information fournie.
            - Réponds UNIQUEMENT avec un objet JSON valide respectant ce schéma :
            {
                "ticketUnderstanding": string,
                "suggestedSteps": string[],
                "possibleCauses": string[],
                "recommendedEscalation": string | null,
                "limitationOrUncertainty": string | null
            }
            Les clés du JSON restent en anglais ; le contenu textuel des valeurs doit être en français.
            """;
    }

    public string BuildUserPrompt(
        TicketContext ticket,
        string technicianQuestion, IReadOnlyList<KnowledgeSearchResult> documentationResults,
        IReadOnlyList<KnowledgeSearchResult> ticketResults)
    {
        var builder = new StringBuilder();

        builder.AppendLine("Informations sur le ticket :");
        builder.AppendLine($"ID: {ticket.IdTicket}");
        builder.AppendLine($"Titre: {ticket.Title}");
        builder.AppendLine($"Application: {ticket.Application}");
        builder.AppendLine($"Category: {ticket.Category}");
        builder.AppendLine($"Statut: {ticket.Status}");
        builder.AppendLine();
        builder.AppendLine("Description:");
        builder.AppendLine(ticket.Description);

        if (ticket.Comments.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Commentaires pertinents (par ordre chronologique) :");

            foreach (var comment in ticket.Comments)
            {
                builder.AppendLine(
                    $"- [{"Unknown"}] {comment.Content}");
            }
        }

        builder.AppendLine();

        if (documentationResults.Count > 0 || ticketResults.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Connaissances internes pertinentes :");

            foreach (var result in documentationResults)
            {
                var label = $"Documentation officielle {ticket.Application}";

                // var resolutionNote = result.ResolutionType switch
                // {
                //     ResolutionType.Workaround => " (WORKAROUND ONLY — root cause not fixed)",
                //     ResolutionType.RootCauseFix => " (root cause fix)",
                //     _ => ""
                // };

                var recurrenceNote = result.RecurrenceCount is > 2
                    ? $" — ce problème s'est déjà reproduit {result.RecurrenceCount} fois."
                    : "";

                builder.AppendLine($"[{result.SourceId}] ({label}){recurrenceNote} {result.Title}");
                builder.AppendLine(result.Content);
                builder.AppendLine();
            }

            foreach (var result in ticketResults)
            {
                var label = $"Ticket résolu antérieurement ({ticket.Application})";

                var resolutionNote = result.RootCauseConfidence switch
                {
                    RootCauseConfidence.QuickFixNoRootCauseFound => " (CONTOURNEMENT UNIQUEMENT — cause racine non corrigée)",
                    RootCauseConfidence.RootCauseIdentified => " (cause racine corrigée)",
                    _ => ""
                };

                var recurrenceNote = result.RecurrenceCount is > 2
                    ? $" — ce problème s'est déjà reproduit {result.RecurrenceCount} fois."
                    : "";

                builder.AppendLine($"[{result.SourceId}] ({label}){resolutionNote}{recurrenceNote} {result.Title}");
                builder.AppendLine(result.Content);
                builder.AppendLine();
            }
        }

        else
        {
            builder.AppendLine();
            builder.AppendLine("Aucune connaissance interne pertinente n'a été trouvée pour ce ticket.");
        }


        builder.AppendLine();
        builder.AppendLine("Demande du technicien :");
        builder.AppendLine(technicianQuestion);

        return builder.ToString();
    }
}
