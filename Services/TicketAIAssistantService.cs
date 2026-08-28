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
    private readonly IAISupportAssistant _aiClient;

    public TicketAIAssistantService(ITicketContextProvider provider, IAISupportAssistant aiClient)
    {
        _provider = provider;
        _aiClient = aiClient;
    }

    public async Task<AISupportResponse> SuggestResponseAsync(int ticketId, AISupportRequest request)
    {
        var context = await _provider.GetForAiAssistanceAsync(ticketId);
        return await _aiClient.GetAssistanceAsync(context, request);
    }
}
