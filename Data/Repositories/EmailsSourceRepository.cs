using SIGRA.Data.Enums;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public sealed class EmailsSourceRepository
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
}
