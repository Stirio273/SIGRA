using MimeKit;
using SIGRA.Data.Models;

namespace SIGRA.Services;

public interface ITicketService
{
    Task<Ticket?> CreateTicketFromEmailAsync(
        MimeMessage message,
        string? conversationId = null,
        CancellationToken cancellationToken = default);
}