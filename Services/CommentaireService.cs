using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIGRA.Data;
using SIGRA.Data.Models;
using SIGRA.Data.Repositories;
using SIGRA.Domain.Exceptions;

namespace SIGRA.Services;

public class CommentaireService : ICommentaireService
{
    private readonly ICommentaireRepository _commentaireRepository;
    private readonly AppDbContext _context;
    private readonly ILogger<CommentaireService> _logger;

    public CommentaireService(ICommentaireRepository commentaireRepository, AppDbContext context, ILogger<CommentaireService> logger)
    {
        _commentaireRepository = commentaireRepository;
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

        await _commentaireRepository.AddAsync(commentaire);
        return commentaire;
    }
}
