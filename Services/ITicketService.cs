using MimeKit;
using SIGRA.Controllers;
using SIGRA.Data.Models;
using SIGRA.Domain;

namespace SIGRA.Services;

public interface ITicketService
{
    Task<Ticket> GetFicheTicket(int idTicket);

    Task<Ticket?> CreateTicketFromEmailAsync(
        MimeMessage message,
        string? conversationId = null,
        CancellationToken cancellationToken = default);

    Task<Ticket> CreateAsync(CreateTicketRequest req);
    Task<Ticket?> GetByIdAsync(int id);
    Task<IReadOnlyList<Ticket>> GetAllAsync();
    Task<IReadOnlyList<Ticket>> GetByTechnicianAsync(Guid technicianUserGuid);
    Task<PagedResult<TicketResponse>> GetPagedAsync(TicketSearchRequest criteria);
    Task<IReadOnlyList<Statut>> GetNextStatutsAsync(int idTicket, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(int id, UpdateTicketRequest req);
    Task<Result> AssignAsync(IEnumerable<int> ticketIds, Guid? technicianUserGuid, string currentUserEmail);
    Task<Result> ReassignAsync(IEnumerable<int> ticketIds, Guid? technicianUserGuid, string justification);
    Task<bool> DeleteAsync(int id);
    Task<Result> AskRejectAsync(int ticketId, int idAuteur, string justificatif);
    Task<Result> CloseAsync(int id);
    Task<PendingRejectResponse?> GetPendingRejectAsync(int ticketId);
    Task<bool> RespondRejectDemandAsync(int ticketId, int idValidateur, bool isRejected);
    Task TransferAsync(int ticketId, int idEntiteExterne, int idAuteur, string explication, bool estDefinitif);
    Task<Result> UpdateApplicationAsync(int ticketId, int? idApplication);
    Task ReopenTicketAsync(ReopenTicketRequest request, int idAuteur);
    Task<IReadOnlyList<Statut>> GetListStatus();
}