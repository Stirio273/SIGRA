using SIGRA.Data.Enums;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public interface IEmailsSourceRepository
{
    Task<EmailsSource> CreateEmailSourceAsync(EmailsSource emailSource, CancellationToken ct = default);
    Task<EmailsSource?> GetByMessageIdAsync(string messageId, CancellationToken ct = default);
    Task<EmailsSource?> GetByConversationIdAsync(string conversationId, CancellationToken ct = default);
}
