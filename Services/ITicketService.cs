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
    Task<IReadOnlyList<Statut>> GetNextStatutsAsync(int idTicket, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int id, UpdateTicketRequest req);
    Task<bool> AssignAsync(IEnumerable<int> ticketIds, Guid? technicianUserGuid, string currentUserEmail);
    Task<bool> DeleteAsync(int id);
    Task AskRejectAsync(int ticketId, int idAuteur, string justificatif);
    Task<bool> RespondRejectDemandAsync(int ticketId, int rejetId, int idValidateur, bool isRejected);
    Task TransferAsync(int ticketId, int idEntiteExterne, int idAuteur, string explication, bool estDefinitif);
}