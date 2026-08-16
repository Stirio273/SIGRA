using System;
using System.Linq.Expressions;
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
            DeadlineResolution = t.DeadlineResolution,
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

    public async Task<PagedResult<TicketResponse>> GetPagedAsync(TicketSearchRequest criteria, CancellationToken ct = default)
    {
        var query = _context.Tickets.AsNoTracking().AsQueryable();

        // Chaque filtre s'applique UNIQUEMENT s'il est fourni
        query = ApplyFilters(query, criteria);
        query = ApplySorting(query, criteria);

        var items = await query
            .Skip((criteria.Pagination.PageNumber - 1) * criteria.Pagination.PageSize)
            .Take(criteria.Pagination.PageSize)
            .Select(t => new TicketResponse
            (
                IdTicket: t.IdTicket,
                NumeroTicket: t.NumeroTicket,
                DateCreation: t.DateCreation,
                Application: t.IdApplicationNavigation == null ? null : new TicketApplicationResponse(t.IdApplicationNavigation.IdApplication, t.IdApplicationNavigation.Libelle, false, 0),
                Criticite: t.IdCriticiteNavigation == null ? null : new TicketCriticiteResponse(t.IdCriticiteNavigation.IdCriticite, t.IdCriticiteNavigation.Libelle, 0),
                Statut: new TicketStatutResponse(t.IdStatutNavigation.IdStatut, t.IdStatutNavigation.Libelle, false),
                TechnicienAssigne: t.IdTechnicienAssigneNavigation == null ? null : new TicketTechnicienResponse(t.IdTechnicienAssigneNavigation.IdUtilisateur, "", "", t.IdTechnicienAssigneNavigation.Email, Guid.Empty),
                DemandeurEmail: t.DemandeurEmail,
                DemandeurDirection: t.DemandeurDirection,
                DateCloture: t.DateCloture,
                DureeSla: t.DureeSla,
                DeadlineResolution: t.DeadlineResolution,
                EmailsSources: null
            ))
            .ToListAsync(ct);

        var totalCount = await query.CountAsync(ct);

        return new PagedResult<TicketResponse>
        {
            Items = items,
            PageNumber = criteria.Pagination.PageNumber,
            PageSize = criteria.Pagination.PageSize,
            TotalCount = totalCount
        };
    }

    private static IQueryable<Ticket> ApplyFilters(
        IQueryable<Ticket> query, TicketSearchRequest request)
    {
        // if (!string.IsNullOrWhiteSpace(request.SearchText))
        // {
        //     var search = request.SearchText.Trim();
        //     query = query.Where(t =>
        //         t.Title.Contains(search) ||
        //         t.Description.Contains(search));
        // }

        if (request.Status.HasValue)
            query = query.Where(t => t.IdStatut == (int)request.Status.Value);

        if (request.Criticite.HasValue)
            query = query.Where(t => t.IdCriticite == (int)request.Criticite.Value);

        if (!string.IsNullOrWhiteSpace(request.ApplicationName))
            query = query.Where(t => t.IdApplicationNavigation.Libelle == request.ApplicationName);

        if (request.AssignedTechnician.HasValue)
            query = query.Where(t => t.IdTechnicienAssigneNavigation.UserGuid == request.AssignedTechnician.Value);

        if (request.CreatedFrom.HasValue)
            query = query.Where(t => t.DateCreation >= request.CreatedFrom.Value.ToUniversalTime());

        if (request.CreatedTo.HasValue)
            query = query.Where(t => t.DateCreation <= request.CreatedTo.Value.ToUniversalTime());

        // if (request.IsOverdue == true)
        //     query = query.Where(t =>
        //         t.SlaDeadline < DateTime.UtcNow && t.Status != TicketStatus.Closed);
        return query;
    }

    private static IQueryable<Ticket> ApplySorting(
        IQueryable<Ticket> query, TicketSearchRequest request)
    {
        Expression<Func<Ticket, object>> keySelector = request.SortBy?.ToLower() switch
        {
            "numeroTicket" => t => t.NumeroTicket,
            "priority" => t => t.IdCriticite,
            "status" => t => t.IdStatut,
            "createdat" => t => t.DateCreation,
            _ => t => t.DateCreation
        };

        return request.SortDescending
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
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
