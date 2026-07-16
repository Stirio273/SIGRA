using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Enums;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public sealed class EmailsSourceRepository : IEmailsSourceRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<EmailsSourceRepository> _logger;

    public EmailsSourceRepository(AppDbContext context, ILogger<EmailsSourceRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<EmailsSource> CreateEmailSourceAsync(EmailsSource emailSource, CancellationToken ct = default)
    {
        _context.EmailsSources.Add(emailSource);
        await _context.SaveChangesAsync(ct);
        return emailSource;
    }

    public async Task<EmailsSource?> GetByMessageIdAsync(string messageId, CancellationToken ct = default)
    {
        return await _context.EmailsSources.FirstOrDefaultAsync(e => e.MessageIdGraph == messageId, ct);
    }

    public async Task<EmailsSource?> GetByConversationIdAsync(string conversationId, CancellationToken ct = default)
    {
        return await _context.EmailsSources.FirstOrDefaultAsync(e => e.ConversationIdGraph == conversationId, ct);
    }
}
