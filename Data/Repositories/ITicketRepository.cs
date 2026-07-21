using Microsoft.EntityFrameworkCore;
using SIGRA.Controllers;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public interface ITicketRepository
{
    Task<long> GetNextSequenceValueAsync();
    Task<Ticket> CreateAsync(Ticket ticket, CancellationToken ct = default);
    Task UpdateAsync(Ticket ticket, CancellationToken ct = default);
    Task<Ticket?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Ticket>> GetByTechnicianAsync(Guid technicianUserGuid, CancellationToken ct = default);
    Task<PagedResult<Ticket>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
