using SIGRA.Domain.AIsupport;

namespace SIGRA.Services;

public sealed class AiSupportOrchestrator : IAISupportOrchestrator
{
    private readonly CompositeKnowledgeRetriever _knowledgeRetriever;
    private readonly IPromptBuilder _promptBuilder;
    private readonly ILlmClient _llmClient;
    private readonly IAIResponseParser _responseParser;
    private readonly ISourceAttacher _sourceAttacher;

    public AiSupportOrchestrator(
        CompositeKnowledgeRetriever knowledgeRetriever,
        IPromptBuilder promptBuilder,
        ILlmClient llmClient,
        IAIResponseParser responseParser,
        ISourceAttacher sourceAttacher)
    {
        _knowledgeRetriever = knowledgeRetriever;
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
        var knowledgeResults = await _knowledgeRetriever.SearchAsync(
            new KnowledgeSearchRequest
            {
                Query = $"{ticket.Title} {ticket.Description}",
                // AllowedModules = request.PreferredKnowledgeDomains,
                IdApplication = ticket.Application,
                ExcludeTicketId = ticket.IdTicket,
                TopK = 5
            },
            cancellationToken);

        var systemPrompt = _promptBuilder.BuildSystemPrompt();
        var userPrompt = _promptBuilder.BuildUserPrompt(
            ticket,
            request.TechnicianQuestion,
            knowledgeResults);

        var rawResponse = await _llmClient.GetCompletionAsync(
            systemPrompt,
            userPrompt,
            cancellationToken);

        var parsedResponse = _responseParser.Parse(rawResponse);

        return _sourceAttacher.Attach(parsedResponse, knowledgeResults);
    }
}
