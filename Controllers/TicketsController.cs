using Microsoft.AspNetCore.Mvc;
using SIGRA.Controllers;
using SIGRA.Data.Models;
using SIGRA.Domain;
using SIGRA.Domain.Exceptions;
using SIGRA.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SIGRA.Controllers;

[ApiController]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly IUserAuthenticationService _userAuthenticationService;
    private readonly ITicketExportService _ticketExportService;
    private readonly ICommentaireService _commentaireService;

    public TicketsController(ITicketService ticketService, IUserAuthenticationService userAuthenticationService, ITicketExportService ticketExportService, ICommentaireService commentaireService)
    {
        _ticketService = ticketService;
        _userAuthenticationService = userAuthenticationService;
        _ticketExportService = ticketExportService;
        _commentaireService = commentaireService;
    }

    [HttpPost("{id}/reopen")]
    public async Task<IActionResult> ReopenTicket(int id, [FromBody] ReopenTicketDto dto)
    {
        try
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var currentUser = await _userAuthenticationService.GetAuthorizedUserAsync(username);
            if (currentUser == null)
                return Unauthorized();


            await _ticketService.ReopenTicketAsync(
                new ReopenTicketRequest { TicketId = id, Reason = dto.Justification },
                currentUser.IdUtilisateur);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("details/{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var ticket = await _ticketService.GetFicheTicket(id);

        if (ticket == null)
        {
            var result = Result.Failure("Ticket not found", ErrorType.NotFound);
            return result.ToHttpResult();
        }

        return Ok(ToResponse(ticket));
    }

    [HttpPost("{id}/transfer")]
    public async Task<IActionResult> Transfer(int id, TransferTicketRequest request)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        var currentUser = await _userAuthenticationService.GetAuthorizedUserAsync(username);
        if (currentUser == null)
            return Unauthorized();

        await _ticketService.TransferAsync(id, request.idEntiteExterne, currentUser.IdUtilisateur, request.explication, request.estDefinitif);
        return Ok();
    }

    [HttpGet("{id:int}/pending-reject")]
    public async Task<IActionResult> GetPendingReject(int id)
    {
        var pending = await _ticketService.GetPendingRejectAsync(id);
        return pending is null ? NotFound() : Ok(pending);
    }

    [HttpPost("request-deny/respond")]
    public async Task<IActionResult> RespondDenyRequest(RespondDenyRequest req)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        var currentUser = await _userAuthenticationService.GetAuthorizedUserAsync(username);
        if (currentUser == null)
            return Unauthorized();

        await _ticketService.RespondRejectDemandAsync(req.IdTicket, currentUser.IdUtilisateur, req.decision);
        return NoContent();
    }

    [HttpPost("request-deny")]
    public async Task<IActionResult> SendDenyRequest(CreateDenyRequest req)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        var currentUser = await _userAuthenticationService.GetAuthorizedUserAsync(username);
        if (currentUser == null)
            return Unauthorized();

        var result = await _ticketService.AskRejectAsync(req.IdTicket, currentUser.IdUtilisateur, req.Justificatif);
        return result.IsSuccess ? NoContent() : result.ToHttpResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTicketRequest req)
    {
        var created = await _ticketService.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = created.IdTicket }, ToResponse(created));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _ticketService.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(ToResponse(item));
    }

    [HttpPatch("{id:int}/close")]
    public async Task<IActionResult> Close(int id)
    {
        var result = await _ticketService.CloseAsync(id);
        return result.IsSuccess ? NoContent() : result.ToHttpResult();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] TicketSearchRequest req)
    {
        var result = await _ticketService.GetPagedAsync(req);
        return Ok(new PagedResult<TicketResponse>
        {
            Items = result.Items,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        });
    }

    [HttpGet("statuts")]
    public async Task<IActionResult> GetAllStatus()
    {
        var result = await _ticketService.GetListStatus();
        return Ok(result);
    }

    [HttpPost("export")]
    public async Task<IActionResult> ExportTickets([FromBody] TicketExportRequest req)
    {
        if (req.From.HasValue && req.To.HasValue)
        {
            if (req.From.Value > req.To.Value)
                return BadRequest(new { message = "La date de début doit être antérieure à la date de fin." });

            if ((req.To.Value - req.From.Value).TotalDays > 365)
                return BadRequest(new { message = "La période ne peut pas dépasser 365 jours." });
        }

        var format = req.Format?.Trim().ToLowerInvariant() ?? "csv";

        byte[] fileBytes;
        try
        {
            fileBytes = await _ticketExportService.ExportTicketsAsync(req.From, req.To, format);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        var fileName = format is "excel" or "xlsx"
            ? $"tickets_{req.From:yyyyMMdd}_{req.To:yyyyMMdd}.xlsx"
            : $"tickets_{req.From:yyyyMMdd}_{req.To:yyyyMMdd}.csv";

        var contentType = format is "excel" or "xlsx"
            ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            : "text/csv";

        return File(fileBytes, contentType, fileName);
    }

    [HttpGet("technician/{technicianUserGuid:guid}")]
    public async Task<IActionResult> GetByTechnician(Guid technicianUserGuid)
    {
        var items = await _ticketService.GetByTechnicianAsync(technicianUserGuid);
        return Ok(items.Select(ToResponse));
    }

    [HttpPatch("assign")]
    public async Task<IActionResult> Assign(AssignTicketsRequest req)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        var result = await _ticketService.AssignAsync(req.TicketIds, req.UserGuid, username);

        return result.IsSuccess ? NoContent() : result.ToHttpResult();
    }

    [HttpPatch("reassign")]
    public async Task<IActionResult> Reassign(ReassignTicketRequest request)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        var result = await _ticketService.ReassignAsync(request.TicketIds, request.UserGuid, request.justification);

        return result.IsSuccess ? Ok() : result.ToHttpResult();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateTicketRequest req)
    {
        var ok = await _ticketService.UpdateAsync(id, req);
        return ok.IsSuccess ? NoContent() : NotFound();
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateApplication(int id, UpdateTicketApplicationRequest req)
    {
        // if (id != req.IdTicket)
        //     return BadRequest();

        var result = await _ticketService.UpdateApplicationAsync(id, req.IdApplication);
        return result.IsSuccess ? NoContent() : result.ToHttpResult();
    }

    [HttpGet("{id:int}/next-statuts")]
    public async Task<IActionResult> GetNextStatuts(int id)
    {
        var statuts = await _ticketService.GetNextStatutsAsync(id);
        return Ok(statuts.Select(s => new StatutSuivantPossibleResponse(s.IdStatut, s.Libelle)));
    }

    [HttpGet("{id:int}/comments")]
    public async Task<IActionResult> GetComments(int id)
    {
        var comments = await _commentaireService.GetByTicketIdAsync(id);
        return Ok(comments.Select(ToCommentaireResponse));
    }

    [HttpPost("{id:int}/comments")]
    public async Task<IActionResult> AddComment(int id, [FromBody] CreateCommentRequest req)
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        var currentUser = await _userAuthenticationService.GetAuthorizedUserAsync(username);
        if (currentUser == null)
            return Unauthorized();

        var commentaire = await _commentaireService.AddAsync(id, currentUser.IdUtilisateur, req.Contenu);
        return CreatedAtAction(nameof(GetComments), new { id = id }, ToCommentaireResponse(commentaire));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _ticketService.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }

    private static TicketResponse ToResponse(Ticket t) => new(
        t.IdTicket,
        t.NumeroTicket,
        t.DateCreation.ToLocalTime(),
        t.IdApplicationNavigation is not null
            ? new TicketApplicationResponse(t.IdApplicationNavigation.IdApplication, t.IdApplicationNavigation.Libelle, t.IdApplicationNavigation.Actif, t.IdApplicationNavigation.IdCs)
            : null,
        t.IdCriticiteNavigation is not null
            ? new TicketCriticiteResponse(t.IdCriticiteNavigation.IdCriticite, t.IdCriticiteNavigation.Libelle, t.IdCriticiteNavigation.Ordre)
            : null,
        new TicketStatutResponse(t.IdStatutNavigation.IdStatut, t.IdStatutNavigation.Libelle, t.IdStatutNavigation.EstDefaut),
        t.IdTechnicienAssigneNavigation is not null
            ? new TicketTechnicienResponse(t.IdTechnicienAssigneNavigation.IdUtilisateur, t.IdTechnicienAssigneNavigation.Nom, t.IdTechnicienAssigneNavigation.Prenom, t.IdTechnicienAssigneNavigation.Email, t.IdTechnicienAssigneNavigation.UserGuid)
            : null,
        t.DemandeurEmail,
        t.DemandeurDirection,
        t.DateCloture?.ToLocalTime(),
        t.DureeSla,
        t.DeadlineResolution?.ToLocalTime(),
        t.EmailsSources is not null ? t.EmailsSources.Select(item => new EmailsSourceResponse
        (
           item.Expediteur,
           item.Objet,
           item.CorpsEmail,
           item.DateReception,
           item.PiecesJointes
     )).ToList() : null);

    private static CommentaireResponse ToCommentaireResponse(Commentaire c) => new(
        c.IdCommentaire,
        c.IdTicket,
        c.IdAuteur,
        c.IdAuteurNavigation.Nom,
        c.IdAuteurNavigation.Prenom,
        c.Contenu,
        c.DateCreation.ToLocalTime());
}
