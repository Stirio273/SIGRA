using SIGRA.Domain.AIsupport;

namespace SIGRA.Services;

public interface IAISupportOrchestrator
{
    Task<AISupportResponse> HandleRequestAsync(
        TicketContext ticket,
        AISupportRequest request,
        CancellationToken cancellationToken = default);
}
