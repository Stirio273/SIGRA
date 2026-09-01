using Microsoft.EntityFrameworkCore;
using SIGRA.Data;
using SIGRA.Domain.AIsupport;
using SIGRA.Services.Providers;

namespace SIGRA.Services;

public interface IAIAssistantService
{
    Task<AISupportResponse> SuggestResponseAsync(int ticketId, AISupportRequest request);
}

public class TicketAIAssistantService : IAIAssistantService
{

    private readonly ITicketContextProvider _provider;
    private readonly IAISupportOrchestrator _orchestrator;

    public TicketAIAssistantService(ITicketContextProvider provider, IAISupportOrchestrator orchestrator)
    {
        _provider = provider;
        _orchestrator = orchestrator;
    }

    public async Task<AISupportResponse> SuggestResponseAsync(int ticketId, AISupportRequest request)
    {
        var context = await _provider.GetForAiAssistanceAsync(ticketId);
        return await _orchestrator.HandleRequestAsync(context, request);
    }
}
