using System;
using Microsoft.EntityFrameworkCore;
using SIGRA.Controllers;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public sealed class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<TicketRepository> _logger;

    public TicketRepository(AppDbContext context, ILogger<TicketRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Ticket?> GetFicheTicket(int idTicket, CancellationToken ct = default)
    {
        return await _context.Tickets
        .AsNoTracking()
        .Where(t => t.IdTicket == idTicket)
        .Select(t => new Ticket
        {
            IdTicket = t.IdTicket,
            NumeroTicket = t.NumeroTicket,
            DateCreation = t.DateCreation,
            IdApplicationNavigation = t.IdApplicationNavigation,
            IdCriticiteNavigation = t.IdCriticiteNavigation,
            IdStatutNavigation = new Statut
            {
                IdStatut = t.IdStatut,
                Libelle = t.IdStatutNavigation.Libelle
            },
            IdTechnicienAssigneNavigation = new Utilisateur
            {
                Nom = t.IdTechnicienAssigneNavigation.Nom,
                Prenom = t.IdTechnicienAssigneNavigation.Prenom,
                Email = t.IdTechnicienAssigneNavigation.Email
            },
            DemandeurEmail = t.DemandeurEmail,
            DemandeurDirection = t.DemandeurDirection,
            DateCloture = t.DateCloture,
            DureeSla = t.DureeSla,
            EmailsSources = t.EmailsSources.Select(e => new EmailsSource
            {
                Expediteur = e.Expediteur,
                Objet = e.Objet,
                CorpsEmail = e.CorpsEmail,
                DateReception = e.DateReception,
                PiecesJointes = e.PiecesJointes
            }).ToList()
        }).FirstOrDefaultAsync();
    }

    public async Task<long> GetNextSequenceValueAsync()
    {
        var result = await _context.Database
            .SqlQuery<long>($"SELECT NEXTVAL('seq_tickets_numero_ticket') \"Value\"")
            .FirstAsync();
        return result;
    }

    public async Task<Ticket> CreateAsync(Ticket ticket, CancellationToken ct = default)
    {
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync(ct);
        return ticket;
    }

    public async Task UpdateAsync(Ticket ticket, CancellationToken ct = default)
    {
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Ticket>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Tickets.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Ticket>> GetByTechnicianAsync(Guid technicianUserGuid, CancellationToken ct = default)
    {
        return await _context.Tickets
            .Where(t => t.IdTechnicienAssigneNavigation!.UserGuid == technicianUserGuid)
            .ToListAsync(ct);
    }

    public async Task<PagedResult<Ticket>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var items = await _context.Tickets
            .OrderBy(t => t.IdTicket)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new Ticket
            {
                IdTicket = t.IdTicket,
                NumeroTicket = t.NumeroTicket,
                DateCreation = t.DateCreation,
                IdApplication = t.IdApplication,
                IdCriticite = t.IdCriticite,
                IdStatut = t.IdStatut,
                IdTechnicienAssigne = t.IdTechnicienAssigne,
                DemandeurEmail = t.DemandeurEmail,
                DemandeurDirection = t.DemandeurDirection,
                DateCloture = t.DateCloture,
                DureeSla = t.DureeSla,
                IdStatutNavigation = new Statut
                {
                    IdStatut = t.IdStatutNavigation.IdStatut,
                    Libelle = t.IdStatutNavigation.Libelle
                },
                IdApplicationNavigation = t.IdApplicationNavigation == null ? null : new Application
                {
                    IdApplication = t.IdApplicationNavigation.IdApplication,
                    Libelle = t.IdApplicationNavigation.Libelle
                },
                IdCriticiteNavigation = t.IdCriticiteNavigation == null ? null : new Criticite
                {
                    IdCriticite = t.IdCriticiteNavigation.IdCriticite,
                    Libelle = t.IdCriticiteNavigation.Libelle
                },
                IdTechnicienAssigneNavigation = t.IdTechnicienAssigneNavigation == null ? null : new Utilisateur
                {
                    IdUtilisateur = t.IdTechnicienAssigneNavigation.IdUtilisateur,
                    Email = t.IdTechnicienAssigneNavigation.Email
                }
            })
            .ToListAsync(ct);

        var totalCount = await _context.Tickets.CountAsync(ct);

        return new PagedResult<Ticket>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.IdTicket == id, ct);
        if (ticket != null)
        {
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<int?> GetIdStatutByDefaultAsync(CancellationToken ct = default)
    {
        return await _context.Statuts
            .AsNoTracking()
            .Where(s => s.EstDefaut)
            .Select(s => (int?)s.IdStatut)
            .FirstOrDefaultAsync(ct);
    }
}
