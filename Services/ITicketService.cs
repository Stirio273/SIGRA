using SIGRA.Data.Models;

namespace SIGRA.Services;

public interface ITicketService
{
    Task<Ticket> CreateTicketFromEmailAsync(
        ImapMailService.MailInfo mailInfo,
        string messageId,
        string? conversationId = null,
        CancellationToken cancellationToken = default);
}