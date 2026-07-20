using Microsoft.EntityFrameworkCore;
using SIGRA.Controllers;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public sealed class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<TicketRepository> _logger;

    public TicketRepository(AppDbContext context, ILogger<TicketRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<long> GetNextSequenceValueAsync()
    {
        var result = await _context.Database
            .SqlQuery<long>($"SELECT NEXTVAL('seq_tickets_numero_ticket') \"Value\"")
            .FirstAsync();
        return result;
    }

    public async Task<Ticket> CreateAsync(Ticket ticket, CancellationToken ct = default)
    {
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync(ct);
        return ticket;
    }

    public async Task UpdateAsync(Ticket ticket, CancellationToken ct = default)
    {
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<Ticket?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Tickets.FirstOrDefaultAsync(t => t.IdTicket == id, ct);
    }

    public async Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Tickets.ToListAsync(ct);
    }

    public async Task<PagedResult<Ticket>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = _context.Tickets.AsQueryable();
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(t => t.IdTicket)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Ticket>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.IdTicket == id, ct);
        if (ticket != null)
        {
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync(ct);
        }
    }
}
