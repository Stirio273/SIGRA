using SIGRA.Domain.AIsupport;

namespace SIGRA.Services;

public interface IPromptBuilder
{
    string BuildSystemPrompt();

    string BuildUserPrompt(
        TicketContext ticket,
        string technicianQuestion,
        IReadOnlyList<KnowledgeSearchResult> knowledgeResults);
}
