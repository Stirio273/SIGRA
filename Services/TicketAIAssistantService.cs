using Microsoft.EntityFrameworkCore;
using SIGRA.Data;
using SIGRA.Domain;

namespace SIGRA.Services;

public interface IAIAssistantService
{
    Task<AISupportResponse> SuggestResponseAsync(int ticketId, AISupportRequest request);
}

public class TicketAIAssistantService : IAIAssistantService
{
    private readonly AppDbContext _db;
    private readonly ITicketContextMapper _mapper;
    private readonly IAISupportAssistant _aiClient;

    public TicketAIAssistantService(AppDbContext db, ITicketContextMapper mapper, IAISupportAssistant aiClient)
    {
        _db = db;
        _mapper = mapper;
        _aiClient = aiClient;
    }

    public async Task<AISupportResponse> SuggestResponseAsync(int ticketId, AISupportRequest request)
    {
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.IdTicket == ticketId);

        var context = _mapper.Map(ticket);
        return await _aiClient.GetAssistanceAsync(context, request);
    }
}
