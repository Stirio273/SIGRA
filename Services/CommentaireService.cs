using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pgvector;
using SIGRA.Data;
using SIGRA.Data.Models;
using SIGRA.Data.Repositories;
using SIGRA.Domain.Exceptions;

namespace SIGRA.Services;

public class CommentaireService : ICommentaireService
{
    private readonly ICommentaireRepository _commentaireRepository;
    private readonly IEmbeddingService _embeddingService;
    private readonly ITicketContentSanitizer _sanitizer;
    private readonly AppDbContext _context;
    private readonly ILogger<CommentaireService> _logger;

    public CommentaireService(ICommentaireRepository commentaireRepository, IEmbeddingService embeddingService, ITicketContentSanitizer sanitizer, AppDbContext context, ILogger<CommentaireService> logger)
    {
        _commentaireRepository = commentaireRepository;
        _embeddingService = embeddingService;
        _sanitizer = sanitizer;
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Commentaire>> GetByTicketIdAsync(int ticketId)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null)
            throw new NotFoundException($"Ticket {ticketId} introuvable.");

        return await _commentaireRepository.GetByTicketIdAsync(ticketId);
    }

    public async Task<Commentaire> AddAsync(int ticketId, int idAuteur, string contenu)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null)
            throw new NotFoundException($"Ticket {ticketId} introuvable.");

        if (string.IsNullOrWhiteSpace(contenu))
            throw new ValidationException("Le contenu du commentaire est obligatoire.");

        var commentaire = new Commentaire
        {
            IdTicket = ticketId,
            IdAuteur = idAuteur,
            Contenu = contenu.Trim(),
            DateCreation = DateTime.UtcNow
        };

        if (ticket.ExclureConnaissancesIa == false && commentaire.EstNoteResolution)
        {
            var embedding = await _embeddingService.EmbedAsync(_sanitizer.Sanitize(commentaire.Contenu), default);
            commentaire.EmbeddingContenu = new Vector(embedding);
        }

        await _commentaireRepository.AddAsync(commentaire);
        return commentaire;
    }
}
