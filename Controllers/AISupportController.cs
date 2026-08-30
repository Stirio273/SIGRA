using Microsoft.AspNetCore.Mvc;
using SIGRA.Services;
using SIGRA.Domain.AIsupport;

namespace SIGRA.Controllers;

[ApiController]
[Route("api/aisupport")]
public class AISupportController : ControllerBase
{
    private readonly IAIAssistantService _aiAssistantService;

    public AISupportController(IAIAssistantService aIAssistantService)
    {
        _aiAssistantService = aIAssistantService;
    }

    [HttpPost("{id}/ask")]
    public async Task<IActionResult> Ask(int id, AskAIRequest request)
    {
        try
        {
            var aiRequest = new AISupportRequest
            {
                TechnicianQuestion = request.Message
            };
            var response = await _aiAssistantService.SuggestResponseAsync(id, aiRequest);

            var condensed = string.Join("\n\n",
                response.TicketUnderstanding,
                response.SuggestedSteps.Count > 0 ? "Suggested Steps:\n" + string.Join("\n", response.SuggestedSteps.Select(s => "- " + s)) : null,
                response.PossibleCauses.Count > 0 ? "Possible Causes:\n" + string.Join("\n", response.PossibleCauses.Select(c => "- " + c)) : null,
                response.RecommendedEscalation,
                response.LimitationOrUncertainty
            );

            return Ok(new { reply = condensed });
        }
        catch (System.Exception e)
        {
            return NotFound(e.Message);
        }
    }
}