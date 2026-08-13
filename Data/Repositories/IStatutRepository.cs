using System;
using System.Threading;
using System.Threading.Tasks;
using SIGRA.Data.Enums;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public interface IStatutRepository
{
    Task<int?> GetIdStatutByDefaultAsync(CancellationToken ct = default);
    Task<int?> GetStatutIdAsync(string libelle, CancellationToken ct = default);
    // Task<bool> IsTransitionAutoriseeAsync(int idStatutOrigine, int idStatutDestination, CancellationToken ct = default);
    Task<IReadOnlyList<Statut>> GetNextStatutsAsync(IReadOnlyList<TicketStatus> ticketStatuses, CancellationToken ct = default);
}
