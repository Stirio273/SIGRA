using SIGRA.Data.Models;
using SIGRA.Domain.AIsupport;

namespace SIGRA.Services.Providers;

public interface ITicketContextProvider
{
    Task<TicketContext?> GetForAiAssistanceAsync(int idTicket, CancellationToken cancellationToken = default);
}
