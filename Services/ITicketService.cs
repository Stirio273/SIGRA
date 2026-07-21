using MimeKit;
using SIGRA.Controllers;
using SIGRA.Data.Models;

namespace SIGRA.Services;

public interface ITicketService
{
    Task<Ticket?> CreateTicketFromEmailAsync(
        MimeMessage message,
        string? conversationId = null,
        CancellationToken cancellationToken = default);

    Task<Ticket> CreateAsync(CreateTicketRequest req);
    Task<Ticket?> GetByIdAsync(int id);
    Task<IReadOnlyList<Ticket>> GetAllAsync();
    Task<IReadOnlyList<Ticket>> GetByTechnicianAsync(Guid technicianUserGuid);
    Task<PagedResult<Ticket>> GetPagedAsync(int pageNumber, int pageSize);
    Task<bool> UpdateAsync(int id, UpdateTicketRequest req);
    Task<bool> DeleteAsync(int id);
}