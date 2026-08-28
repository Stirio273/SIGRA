using SIGRA.Domain.AIsupport;

namespace SIGRA.Services;

public interface IAISupportAssistant
{
    Task<AISupportResponse> GetAssistanceAsync(
        TicketContext ticket,
        AISupportRequest request,
        CancellationToken cancellationToken = default);
}
