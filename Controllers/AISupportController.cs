using Microsoft.AspNetCore.Mvc;
using SIGRA.Services;
using SIGRA.Domain.AIsupport;

namespace SIGRA.Controllers;

[ApiController]
[Route("api/tickets")]
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
            var response = _aiAssistantService.SuggestResponseAsync(id, aiRequest);
            return Ok(response);
        }
        catch (System.Exception e)
        {
            return NotFound(e.Message);
        }
    }
}