using SIGRA.Domain;

namespace SIGRA.Services;

public interface IAISupportAssistant
{
    Task<AISupportResponse> GetAssistanceAsync(
        TicketContext ticket,
        AISupportRequest request,
        CancellationToken cancellationToken = default);
}
