using SIGRA.Data.Models;
using SIGRA.Domain.AIsupport;

namespace SIGRA.Services;

public sealed class AiSupportOrchestrator : IAISupportOrchestrator
{
    private readonly IDocumentationKnowledgeRetriever _documentationKnowledgeRetriever;
    private readonly IResolvedTicketKnowledgeRetriever _resolvedTicketKnowledgeRetriever;
    private readonly IPromptBuilder _promptBuilder;
    private readonly ILlmClient _llmClient;
    private readonly IAIResponseParser _responseParser;
    private readonly ISourceAttacher _sourceAttacher;

    public AiSupportOrchestrator(
        IDocumentationKnowledgeRetriever documentationKnowledgeRetriever,
        IResolvedTicketKnowledgeRetriever resolvedTicketKnowledgeRetriever,
        IPromptBuilder promptBuilder,
        ILlmClient llmClient,
        IAIResponseParser responseParser,
        ISourceAttacher sourceAttacher)
    {
        _documentationKnowledgeRetriever = documentationKnowledgeRetriever;
        _resolvedTicketKnowledgeRetriever = resolvedTicketKnowledgeRetriever;
        _promptBuilder = promptBuilder;
        _llmClient = llmClient;
        _responseParser = responseParser;
        _sourceAttacher = sourceAttacher;
    }

    public async Task<AISupportResponse> HandleRequestAsync(
        TicketContext ticket,
        AISupportRequest request,
        CancellationToken cancellationToken = default)
    {
        var knowledgeRequest = new KnowledgeSearchRequest
        {
            Query = $"{ticket.Title} {ticket.Description}",
            // AllowedModules = request.PreferredKnowledgeDomains,
            Application = new Application
            {
                Libelle = ticket.Application ?? ""
            },
            ExcludeTicketId = ticket.IdTicket,
            TopK = 5
        };
        var ticketResults = await _resolvedTicketKnowledgeRetriever.SearchResolvedTicketsAsync(knowledgeRequest, cancellationToken);
        var docResults = await _documentationKnowledgeRetriever.SearchDocumentationAsync(knowledgeRequest, cancellationToken);

        var systemPrompt = _promptBuilder.BuildSystemPrompt();
        var userPrompt = _promptBuilder.BuildUserPrompt(
            ticket,
            request.TechnicianQuestion,
            docResults,
            ticketResults);

        var rawResponse = await _llmClient.GetCompletionAsync(
            systemPrompt,
            userPrompt,
            cancellationToken);

        var parsedResponse = _responseParser.Parse(rawResponse);

        return _sourceAttacher.Attach(parsedResponse, docResults, ticketResults);
    }
}
