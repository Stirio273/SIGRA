using SIGRA.Data;
using Microsoft.EntityFrameworkCore;
using SIGRA.Domain.AIsupport;
using SIGRA.Data.Models;

namespace SIGRA.Services.Providers;

public class TicketContextProvider : ITicketContextProvider
{
    private readonly AppDbContext _db;
    private readonly ITicketContentSanitizer _sanitizer;

    public TicketContextProvider(AppDbContext db, ITicketContentSanitizer sanitizer)
    {
        _db = db;
        _sanitizer = sanitizer;
    }

    public async Task<TicketContext?> GetForAiAssistanceAsync(int idTicket, CancellationToken cancellationToken = default)
    {
        var ticket = await _db.Tickets
            .AsNoTracking()
            .Include(t => t.IdApplicationNavigation)
                .ThenInclude(a => a.IdCsNavigation)
                    .ThenInclude(cs => cs.IdCriticiteNavigation)
            .Include(t => t.IdCriticiteNavigation)
            .Include(t => t.IdStatutNavigation)
            .FirstOrDefaultAsync(t => t.IdTicket == idTicket, cancellationToken);

        if (ticket == null)
        {
            throw new Exception($"Ticket not found {idTicket}");
        }

        var comments = await _db.Commentaires
            .AsNoTracking()
            .Where(c => c.IdTicket == idTicket)
            .OrderBy(c => c.DateCreation)
            .Select(c => new TicketCommentContext
            {
                Content = c.Contenu,
                CreatedAt = new DateTimeOffset(c.DateCreation, TimeSpan.Zero)
            })
            .ToListAsync(cancellationToken);

        var emails = await _db.EmailsSources
            .AsNoTracking()
            .Where(e => e.IdTicket == idTicket)
            .ToListAsync(cancellationToken);

        var initialEmail = emails.FirstOrDefault(e => e.EstEmailInitial);
        var title = initialEmail != null && !string.IsNullOrWhiteSpace(initialEmail.Objet)
            ? _sanitizer.Sanitize(initialEmail.Objet)
            : _sanitizer.Sanitize(ticket.NumeroTicket);

        var emailBodies = emails
            .Select(e => e.CorpsEmail)
            .Where(body => !string.IsNullOrWhiteSpace(body))
            .ToList();

        var description = emailBodies.Count > 0
            ? _sanitizer.Sanitize(string.Join("\n\n", emailBodies))
            : _sanitizer.Sanitize(ticket.DemandeurDirection);

        var priority = ticket.IdCriticiteNavigation != null
            ? ticket.IdCriticiteNavigation.Libelle
            : ticket.IdApplicationNavigation?.IdCsNavigation?.IdCriticiteNavigation?.Libelle;

        return new TicketContext
        {
            IdTicket = idTicket,
            Title = title,
            Description = description,
            Application = ticket.IdApplicationNavigation != null ? ticket.IdApplicationNavigation.Libelle : null,
            Category = string.Empty,
            Priority = priority != null ? _sanitizer.Sanitize(priority) : null,
            Status = ticket.IdStatutNavigation != null ? ticket.IdStatutNavigation.Libelle : null,
            CreatedAt = new DateTimeOffset(ticket.DateCreation, TimeSpan.Zero),
            Comments = comments,
            Metadata = CreateMetadata(ticket)
        };
    }

    private static IReadOnlyDictionary<string, string> CreateMetadata(Ticket ticket)
    {
        var metadata = new Dictionary<string, string>();

        AddIfPresent(metadata, "Application", ticket.IdApplicationNavigation?.Libelle);
        AddIfPresent(metadata, "Priority", ticket.IdCriticiteNavigation?.Libelle);
        AddIfPresent(metadata, "Status", ticket.IdStatutNavigation?.Libelle);

        return metadata;
    }

    private static void AddIfPresent(
        IDictionary<string, string> metadata,
        string key,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            metadata[key] = value;
        }
    }
}