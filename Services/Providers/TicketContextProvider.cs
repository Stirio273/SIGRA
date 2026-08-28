using SIGRA.Data;
using Microsoft.EntityFrameworkCore;
using SIGRA.Domain.AIsupport;
using ScottPlot;
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
        var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.IdTicket == idTicket);
        if (ticket == null)
        {
            throw new Exception($"Ticket not found {idTicket}");
        }

        var comments = ticket.Commentaires.OrderBy(c => c.DateCreation).Select(c => new TicketCommentContext
        {
            Content = c.Contenu
        }).ToList();

        return new TicketContext
        {
            IdTicket = idTicket,
            Title = _sanitizer.Sanitize(""),
            Description = _sanitizer.Sanitize(""),
            Application = ticket.IdApplicationNavigation.Libelle,
            Category = "",
            Priority = ticket.IdApplicationNavigation.IdCsNavigation.IdCriticiteNavigation.Libelle,
            Status = ticket.IdStatutNavigation.Libelle,
            CreatedAt = DateTime.UtcNow,
            Comments = comments,
            Metadata = CreateMetadata(ticket)
        };
    }

    private static IReadOnlyDictionary<string, string> CreateMetadata(Ticket ticket)
    {
        var metadata = new Dictionary<string, string>();

        AddIfPresent(metadata, "Application", ticket.IdApplicationNavigation.Libelle);
        // AddIfPresent(metadata, "Category", ticket.CategoryName);
        AddIfPresent(metadata, "Priority", ticket.IdApplicationNavigation.IdCsNavigation.IdCriticiteNavigation.Libelle);
        AddIfPresent(metadata, "Status", ticket.IdStatutNavigation.Libelle);
        // AddIfPresent(metadata, "Environment", ticket.EnvironmentName);

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