// using System.Text.Json;
// using SIGRA.Domain.AIsupport;

// namespace SIGRA.Services;

// public sealed class BasicTicketContextAssistant : IAISupportAssistant
// {
//     private readonly ILlmClient _llmClient;
//     private readonly IKnowledgeRetriever _knowledgeRetriever;

//     public BasicTicketContextAssistant(
//         ILlmClient llmClient,
//         IKnowledgeRetriever knowledgeRetriever)
//     {
//         _llmClient = llmClient;
//         _knowledgeRetriever = knowledgeRetriever;
//     }

//     public async Task<AISupportResponse> GetAssistanceAsync(
//         TicketContext ticket,
//         AISupportRequest request,
//         CancellationToken cancellationToken = default)
//     {
//         var searchResults = await _knowledgeRetriever.SearchAsync(
//             new KnowledgeSearchRequest
//             {
//                 Query = $"{ticket.Title} {ticket.Description}",
//                 AllowedModules = request.PreferredKnowledgeDomains,
//                 TopK = 5
//             },
//             cancellationToken);

//         var systemPrompt = TicketPromptBuilder.BuildSystemPrompt();
//         var userPrompt = TicketPromptBuilder.BuildUserPrompt(
//             ticket,
//             request.TechnicianQuestion, searchResults);

//         var rawResponse = await _llmClient.GetCompletionAsync(
//             systemPrompt,
//             userPrompt,
//             cancellationToken);

//         var response = ParseResponse(rawResponse);
//         response.Sources = searchResults
//                 .Select(result => new AISourceReference
//                 {
//                     SourceType = "InternalDocument",
//                     SourceId = result.SourceId,
//                     Title = result.Title,
//                     Excerpt = Truncate(result.Content, 200),
//                     RelevanceScore = result.Score
//                 })
//                 .ToList();

//         return response;
//     }

//     private static string Truncate(string text, int maxLength) =>
//        text.Length <= maxLength ? text : text[..maxLength] + "...";

//     private static AISupportResponse ParseResponse(string rawResponse)
//     {
//         try
//         {
//             var parsed = JsonSerializer.Deserialize<LlmResponseDto>(
//                 rawResponse,
//                 new JsonSerializerOptions
//                 {
//                     PropertyNameCaseInsensitive = true
//                 });

//             if (parsed is null)
//             {
//                 return FallbackResponse(rawResponse);
//             }

//             return new AISupportResponse
//             {
//                 TicketUnderstanding = parsed.TicketUnderstanding
//                     ?? "No understanding was provided.",
//                 SuggestedSteps = parsed.SuggestedSteps ?? [],
//                 PossibleCauses = parsed.PossibleCauses ?? [],
//                 RecommendedEscalation = parsed.RecommendedEscalation,
//                 LimitationOrUncertainty = parsed.LimitationOrUncertainty
//                     ?? "This answer is based only on ticket content; no knowledge base was consulted.",
//                 Sources = []
//             };
//         }
//         catch (JsonException)
//         {
//             return FallbackResponse(rawResponse);
//         }
//     }

//     private sealed class LlmResponseDto
//     {
//         public string? TicketUnderstanding { get; set; }
//         public List<string>? SuggestedSteps { get; set; }
//         public List<string>? PossibleCauses { get; set; }
//         public string? RecommendedEscalation { get; set; }
//         public string? LimitationOrUncertainty { get; set; }
//     }
// }
