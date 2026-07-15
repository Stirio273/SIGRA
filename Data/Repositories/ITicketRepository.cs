using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public interface ITicketRepository
{
    Task<long> GetNextSequenceValueAsync();
    Task<Ticket> CreateAsync(Ticket ticket, CancellationToken ct = default);
    Task UpdateAsync(Ticket ticket, CancellationToken ct = default);
}
